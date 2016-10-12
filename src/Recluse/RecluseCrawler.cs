using System.Threading.Tasks;
using Recluse.Core.Document;
using Recluse.Core.Processing;

namespace Recluse
{
    public class RecluseCrawler
    {
        private readonly IUriProcessor _processor;

        public RecluseCrawler(RecluseCrawlerOptions options, IUriProcessor processor)
        {
            _processor = processor;
            _processor.SetOptions(options.ProcessorOptions);
        }

        public Task<WebDocument> CrawlAsync(ICrawlTask task)
        {
            return _processor.CrawlAsync((task));
        }
        /// <summary>
        /// Blocking call to stop crawler. It will complete the working queue.
        /// </summary>
        public void Stop()
        {
            _processor.Stop();
        }
        public void Start()
        {
            _processor.Start();
        }
    }
}
