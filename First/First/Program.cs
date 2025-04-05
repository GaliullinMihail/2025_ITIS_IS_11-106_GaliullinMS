using First;

 class Program
{
    public async static Task Main()
    {
        var crawler = new WebCrawler();
        var linkList = new List<string>()
        {
            "https://ru.wikipedia.org/wiki/Текст"
        };
        await crawler.CrawlAsync(linkList);
    }
}