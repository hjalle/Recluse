using System.Threading.Tasks;
using Recluse.Core.Document;

namespace Recluse.Core.Processing
{
    public interface IUriProcessor
    {
        void SetOptions(ProcessorOptions options);
        void Start();
        void Stop();
        bool IsProcessing { get; }
        Task<WebDocument> CrawlAsync(ICrawlTask task);
    }
}
