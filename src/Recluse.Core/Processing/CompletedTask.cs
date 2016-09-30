using Recluse.Core.Document;

namespace Recluse.Core.Processing
{
    public class CompletedTask
    {
        public WebDocument FetchedDocument { get; set; }
        public ICrawlTask Task { get; set; }  
    }
}
