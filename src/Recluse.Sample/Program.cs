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

            var task = crawler.CrawlAsync(new CrawlTask(new Uri("http://www.ycombinator.com")));

            crawler.Start();
            task.Wait();
            WriteDownloadedPage(task.Result);
            Console.ReadLine();
            crawler.Stop();
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
