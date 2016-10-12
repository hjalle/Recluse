using System;
using Microsoft.Extensions.DependencyInjection;
using Recluse.Core.Document;
using Recluse.Core.Handlers;
using Recluse.Core.Processing;
using Recluse.Extensions;

namespace Recluse.Sample
{
    public class Program
    {

        public static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<ICrawlHandler, LogCrawlHandler>();

            services.AddRecluseCrawler();
            
            var serviceProvider = services.BuildServiceProvider();
            var crawler = serviceProvider.GetService<RecluseCrawler>();

            crawler.Start();
            var task = crawler.CrawlAsync(new CrawlTask(new Uri("http://www.ycombinator.com")));
            var task2 = crawler.CrawlAsync(new CrawlTask(new Uri("http://news.ycombinator.com")));

            crawler.Stop();
            task.Wait();
            WriteDownloadedPage(task2.Result);
            Console.ReadLine();
        }

        private static void WriteDownloadedPage(WebDocument obj)
        {
            Console.WriteLine($"{obj.Uri} -  {obj.StatusCode} - {obj.Headers}");
            foreach (var item in obj.Links)
            {
                Console.WriteLine($"{item.Uri} -  {item.LinkText} - {item.LinkType}");
            }
        }
    }
}
