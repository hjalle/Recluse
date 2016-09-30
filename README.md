# Recluse
[![Build status](https://ci.appveyor.com/api/projects/status/f98p4bm9gvxay433/branch/master?svg=true)](https://ci.appveyor.com/project/hjalle/recluse/branch/master)
[![MyGet CI](https://img.shields.io/myget/hjalle/v/Recluse.svg)](http://myget.org/gallery/hjalle)
[![NuGet](https://img.shields.io/nuget/v/Recluse.svg)](https://www.nuget.org/packages/Recluse/)



Recluse is a simple dotnet core web crawler.

Usage from Console app:
```cs
IServiceCollection services = new ServiceCollection();

services.AddSingleton<ICrawlHandler, LogCrawlHandler>();
services.AddRecluseCrawler();
            
var serviceProvider = services.BuildServiceProvider();
var crawler = serviceProvider.GetService<RecluseCrawler>();

var task = crawler.CrawlAsync(new CrawlTask(new Uri("http://news.ycombinator.com")));

crawler.Start();
task.Wait();
var obj = task.Result;
Console.WriteLine($"{obj.Uri} -  {obj.StatusCode} - {obj.Headers}");
foreach (var item in obj.Links)
{
    Console.WriteLine($"{item.Uri} -  {item.LinkText} - {item.LinkType}");
}
```

