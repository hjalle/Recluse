using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Recluse.Core.Document;
using Recluse.Core.Events;
using Recluse.Core.Handlers;
using Recluse.Core.Repository;

namespace Recluse.Core.Processing
{
    public class UriProcessor : IUriProcessor
    {
        private readonly IList<Task<UriFetcher>> _executing = new List<Task<UriFetcher>>();
        private readonly ICrawlTaskRepository _queue;
        private ConcurrentQueue<ICrawlTask> _workingQueue = new ConcurrentQueue<ICrawlTask>();
        private bool _isProcessing;

        private readonly Action<WebDocument> _onDocumentFetched;

        private ProcessorOptions _options;
        private Thread _processingThread;
        private Thread _fetchingThread;


        public UriProcessor(ICrawlTaskRepository queue, ICrawlHandler handler)
        {
            _queue = queue;
            _immidiateCrawls = new ConcurrentDictionary<ICrawlTask, AsyncManualResetEvent>();
            _immidiateCompletedTasks = new ConcurrentDictionary<ICrawlTask, WebDocument>();
            if (handler != null)
            {
                _onDocumentFetched = handler.OnDocumentFetched;
            }
        }


        public void SetOptions(ProcessorOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Process the downloader queue.
        /// </summary>
        public void Start()
        {
            if (_isProcessing)
            {
                throw new InvalidOperationException("Can't Process if already processing");
            }
            ThreadStart fetchingThread = () =>
            {
                bool isFetching = false;
                while (_isProcessing)
                {
                    if (_workingQueue.Count < _options.LimitBeforeNewFetch)
                    {
                        if (!isFetching)
                        {
                            var fetchTask = _queue.FetchAsync(_options.MaxAmountToFetch);
                            isFetching = true;
                            fetchTask.ContinueWith(taskFetched =>
                            {
                                foreach (var item in taskFetched.Result)
                                {
                                    _workingQueue.Enqueue(item);
                                }
                                isFetching = false;
                            });
                        }
                    }
                }
            };
            ThreadStart processingThreadStart = () =>
            {
                while (_isProcessing)
                {
                    var completed = _executing.Where(e => e.IsCompleted || e.IsCanceled || e.IsFaulted).ToList();
                    foreach (var item in completed)
                    {
                        if (item.IsCompleted)
                        {
                            WorkDone(item.Result.CompletedTask);
                        }
                        _executing.Remove(item);
                    }
                    if (_workingQueue.Count > 0)
                    {
                        var tasksToCreate = Math.Min(_options.MaxConcurrency - _executing.Count, _workingQueue.Count);
                        for (var i = 0; i < tasksToCreate; i++)
                        {
                            ICrawlTask crawlTask;
                            _workingQueue.TryDequeue(out crawlTask);
                            if (crawlTask != null)
                            {
                                UriFetcher fetcher = new UriFetcher(crawlTask);
                                var result = fetcher.Fetch();
                                _executing.Add(result);
                            }
                        }
                    }
                }
                if (_executing != null)
                {
                    Task.WaitAll(_executing.ToArray());
                }
            };


            _fetchingThread = new Thread(fetchingThread);
            _fetchingThread.Start();
            _processingThread = new Thread(processingThreadStart);
            _processingThread.Start();

            _isProcessing = true;

        }
        private void WorkDone(CompletedTask resultCompletedTask)
        {
            if (_immidiateCrawls[resultCompletedTask.Task] != null)
            {
                var manualEvent = _immidiateCrawls[resultCompletedTask.Task];
                _immidiateCompletedTasks[resultCompletedTask.Task] = resultCompletedTask.FetchedDocument;
                manualEvent.Set();
            }
            _onDocumentFetched(resultCompletedTask.FetchedDocument);
        }

        public bool IsProcessing => _isProcessing;

        private ConcurrentDictionary<ICrawlTask, AsyncManualResetEvent> _immidiateCrawls;
        private ConcurrentDictionary<ICrawlTask, WebDocument> _immidiateCompletedTasks;
        public async Task<WebDocument> CrawlAsync(ICrawlTask task)
        {
            _workingQueue.Enqueue(task);
            var manualEvent = new AsyncManualResetEvent();
            _immidiateCrawls[task] = manualEvent;
            await manualEvent.WaitAsync();
            _immidiateCrawls.TryRemove(task, out manualEvent);
            WebDocument doc;
            _immidiateCompletedTasks.TryRemove(task, out doc);
            return doc;

        }

        public void Stop()
        {
            if (!_isProcessing)
            {
                throw new InvalidOperationException("Can't stop if not processing");
            }
            _isProcessing = false;
            _fetchingThread.Join();
            _processingThread.Join();
        }
    }

}
