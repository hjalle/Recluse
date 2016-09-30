using System.Collections.Generic;
using System.Threading.Tasks;
using Recluse.Core.Processing;

namespace Recluse.Core.Repository
{
    public interface ICrawlTaskRepository
    {
        Task<List<ICrawlTask>> FetchAsync(int maxAmount);
    }
}
