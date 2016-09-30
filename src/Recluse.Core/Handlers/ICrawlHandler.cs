using Recluse.Core.Document;

namespace Recluse.Core.Handlers
{
    public interface ICrawlHandler
    {
        void OnDocumentFetched(WebDocument obj);
    }
}
