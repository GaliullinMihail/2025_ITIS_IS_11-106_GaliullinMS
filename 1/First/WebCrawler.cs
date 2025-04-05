namespace First;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

public class WebCrawler
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentBag<string> _visitedUrls;
    private readonly ConcurrentDictionary<int, string> _downloadedPages;
    private readonly int _minPages;
    private readonly int _minWords;
    private readonly string _outputDirectory;
    private int _currentDocId;
    private readonly double _matchRussian;
    private readonly string _startDirectory;

    public WebCrawler(int minPages = 100, int minWords = 1000, double matchRussian = 0.5,string outputDirectory = "pages")
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; WebCrawler/1.0)");
        _visitedUrls = new ConcurrentBag<string>();
        _downloadedPages = new ConcurrentDictionary<int, string>();
        _minPages = minPages;
        _minWords = minWords;
        _outputDirectory = outputDirectory;
        _currentDocId = 0;
        _matchRussian = matchRussian;
        _startDirectory = "D:\\Учеба\\Information_Search\\git\\2025_ITIS_IS_11-106_GaliullinMS";

        Directory.CreateDirectory(Path.Combine(_startDirectory, _outputDirectory));
    }

    public async Task CrawlAsync(IEnumerable<string> startUrls)
    {
        var queue = new ConcurrentQueue<string>(startUrls);
        var tasks = new List<Task>();

        while (_downloadedPages.Count < _minPages && queue.TryDequeue(out var url))
        {
            if (_visitedUrls.Contains(url)) continue;
            _visitedUrls.Add(url);

            tasks.Add(ProcessUrlAsync(url, queue));
            
            //ограничение одновременных
            if (tasks.Count >= 10)
            {
                await Task.WhenAny(tasks);
                tasks.RemoveAll(t => t.IsCompleted);
            }
            await Task.WhenAll(tasks);
        }
        SaveIndexFile();
    }

    private async Task ProcessUrlAsync(string url, ConcurrentQueue<string> queue)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            // Получить html док
            doc.LoadHtml(html);

            // Получить текст
            var text = ExtractText(doc);
            
            // Проверить язык и длину текста 
            if (IsRussian(text) && WordCount(text) >= _minWords)
            {
                var docId = Interlocked.Increment(ref _currentDocId);
                _downloadedPages.TryAdd(docId, url);
                
                // Сохранить текст в файл
                await File.WriteAllTextAsync(Path.Combine(Path.Combine(_startDirectory, _outputDirectory), $"{docId}.txt"), text);
            }

            // Не набрали нужное кол-во берем дочерние
            if (_downloadedPages.Count < _minPages)
            {
                var links = ExtractLinks(doc, url);
                foreach (var link in links)
                {
                    if (!_visitedUrls.Contains(link))
                    {
                        queue.Enqueue(link);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке {url}: {ex.Message}");
        }
    }

    private string ExtractText(HtmlDocument doc)
    {
        var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//nav|//footer|//head|//iframe//button|//noscript|//form|//svg|//comment()");
        if (nodesToRemove != null)
        {
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }
        var text = doc.DocumentNode.InnerText;
        
        // Удалить всё если не кириллица или пробел
        text = Regex.Replace(text, @"[^а-яА-ЯёЁ\s]", " ");
    
        // Заменить множественные пробелы
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text;
    }

    private IEnumerable<string> ExtractLinks(HtmlDocument doc, string baseUrl)
    {
        var links = new HashSet<string>();
        //получить <a> если есть href любой тип вложенности
        var anchorNodes = doc.DocumentNode.SelectNodes("//a[@href]");

        if (anchorNodes != null)
        {
            foreach (var node in anchorNodes)
            {
                var href = node.Attributes["href"].Value;
                if (!string.IsNullOrWhiteSpace(href) && !href.StartsWith("#"))  //убрать пустые или локальные
                {
                    try
                    {
                        var absoluteUrl = new Uri(new Uri(baseUrl), href).AbsoluteUri;
                        if (IsValidUrl(absoluteUrl))
                        {
                            links.Add(absoluteUrl);
                        }
                    }
                    catch { }
                }
            }
        }

        return links;
    }

    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    // первые 500 символом проверяем на [А-я] > _matchRussian
    private bool IsRussian(string text)
    {
        var sample = text.Length > 500 ? text.Substring(0, 500) : text;
        var cyrillicCount = sample.Count(c => c >= 'А' && c <= 'я');
        return (double)cyrillicCount / sample.Length > _matchRussian;
    }

    private int WordCount(string text)
    {
        return Regex.Matches(text, @"[\p{L}]+").Count;  //\p{L} - любая буква любого языка
    }

    // сохранить index файл с ссылками
    private void SaveIndexFile()
    {
        var indexLines = _downloadedPages.OrderBy(x => x.Key)
            .Select(x => $"{x.Key}\t{x.Value}");
        
        File.WriteAllLines(Path.Combine(System.IO.Path.Combine(_startDirectory, _outputDirectory), "index.txt"), indexLines);
    }
}