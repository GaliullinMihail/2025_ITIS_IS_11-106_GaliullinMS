using First;

 class Program
{
    public async static Task Main()
    {
        var crawler = new WebCrawler();
        var linkList = new List<string>()
        {
            "https://ru.ruwiki.ru/wiki/Заглавная_страница"
        };
        await crawler.CrawlAsync(linkList);
    }
}