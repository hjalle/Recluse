using System;
using Recluse.Core.Document;
using Recluse.Core.Handlers;

namespace Recluse.Sample
{
    public class LogCrawlHandler : ICrawlHandler
    {
        public void OnDocumentFetched(WebDocument obj)
        {
            Console.WriteLine($"LogCrawlHandler: Fetched {obj.Uri}");
        }
    }
}
