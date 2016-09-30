using System;

namespace Recluse.Core.Processing
{
    public class CrawlTask : ICrawlTask
    {
        public CrawlTask(Uri uri)
        {
            Uri = uri;
        }
        public Uri Uri
        {
            get; set;
        }
    }
}
