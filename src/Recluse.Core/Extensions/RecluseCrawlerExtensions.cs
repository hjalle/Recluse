using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Recluse.Core.Processing;
using Recluse.Core.Repository;

namespace Recluse.Core.Extensions
{
    public static class RecluseCrawlerExtensions
    {
        public static void AddRecluseCrawler(this IServiceCollection serviceCollection, Action<RecluseCrawlerOptions> setupAction = null)
        {
            var options = new RecluseCrawlerOptions();
            setupAction?.Invoke(options);
            if (options.ProcessorOptions == null)
            {
                options.ProcessorOptions = new ProcessorOptions();
            }
            serviceCollection.AddSingleton(options);
            serviceCollection.TryAddSingleton(typeof(IUriProcessor), typeof(UriProcessor));
            serviceCollection.TryAddSingleton(typeof(ICrawlTaskRepository), typeof(InMemoryCrawlTaskRepository));
            serviceCollection.AddSingleton<IUriProcessor,UriProcessor>();
            serviceCollection.AddSingleton<RecluseCrawler,RecluseCrawler>();

        }
    }
}
