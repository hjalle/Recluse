# Recluse

Recluse is a simple dotnet core web crawler.

Usage from Console app:
```cs
IServiceCollection services = new ServiceCollection();

services.AddSingleton<ICrawlHandler, LogCrawlHandler>();
services.AddRecluseCrawler();
            
var serviceProvider = services.BuildServiceProvider();
var crawler = serviceProvider.GetService<RecluseCrawler>();

var task = crawler.CrawlAsync(new CrawlTask(new Uri("http://www.ycombinator.com")));

crawler.Start();
task.Wait();
var obj = task.Result;
Console.WriteLine($"{obj.Uri} -  {obj.StatusCode} - {obj.Headers}");
foreach (var item in obj.Links)
{
    Console.WriteLine($"{item.Uri} -  {item.LinkText} - {item.LinkType}");
}
```

