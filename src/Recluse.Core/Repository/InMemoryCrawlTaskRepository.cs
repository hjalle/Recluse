using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Recluse.Core.Processing;

namespace Recluse.Core.Repository
{
    public class InMemoryCrawlTaskRepository : ICrawlTaskRepository
    {
        private List<ICrawlTask> _taskList;

        public InMemoryCrawlTaskRepository()
        {
            _taskList = new List<ICrawlTask>();
        }
        public void Add(string uriStr)
        {
            Uri uri = new Uri(uriStr);
            var task = new CrawlTask(uri);
            Add(task);
        }
        public void Add(ICrawlTask task)
        {
            _taskList.Add(task);
        }
        public Task<List<ICrawlTask>> FetchAsync(int maxAmount)
        {
            return Task.Run(() =>
            {
                var result = _taskList.Take(maxAmount).ToList();
                _taskList = _taskList.Skip(maxAmount).ToList();
                return result;
            });
        }
    }
}
