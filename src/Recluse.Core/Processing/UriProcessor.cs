using System;
using System.Collections.Concurrent;
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
        private readonly ICrawlTaskRepository _queue;
        private readonly ConcurrentQueue<ICrawlTask> _workingQueue = new ConcurrentQueue<ICrawlTask>();
        private Semaphore _workerSemaphore;

        private readonly ManualResetEvent _processWork = new ManualResetEvent(false);

        private bool _isFetching;
        private readonly Action<WebDocument> _onDocumentFetched;
        private readonly ManualResetEvent _waitHandle;
        public bool IsProcessing { get; private set; }

        private readonly ConcurrentDictionary<ICrawlTask, AsyncManualResetEvent> _immidiateCrawls;
        private readonly ConcurrentDictionary<ICrawlTask, WebDocument> _immidiateCompletedTasks;
        private Timer _fetchCrawlTaskTimer;
        private ProcessorOptions _options;
        private Thread _processingThread;

        private readonly object _syncLock = new object();

        public UriProcessor(ICrawlTaskRepository queue, ICrawlHandler handler)
        {
            _queue = queue;
            _immidiateCrawls = new ConcurrentDictionary<ICrawlTask, AsyncManualResetEvent>();
            _immidiateCompletedTasks = new ConcurrentDictionary<ICrawlTask, WebDocument>();
            if (handler != null)
            {
                _onDocumentFetched = handler.OnDocumentFetched;
            }
            _waitHandle = new ManualResetEvent(false); 
        }


        public void SetOptions(ProcessorOptions options)
        {
            _options = options;
            _workerSemaphore = new Semaphore(_options.MaxConcurrency, _options.MaxConcurrency);
        }

        /// <summary>
        /// Process the downloader queue.
        /// </summary>
        public void Start()
        {
            var start = new ManualResetEvent(false);
            ThreadStart processingThreadStart = () =>
            {

                bool signaled;
                lock (_syncLock)
                {
                    IsProcessing = true;
                }
                start.Set();
                _fetchCrawlTaskTimer = new Timer(FetchCrawlTasks, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
                do
                {
                    _processWork.WaitOne();
                    RunWorkers();
                    signaled = _waitHandle.WaitOne(0);
                } while (!signaled);
                lock (_syncLock)
                {
                    IsProcessing = false;
                }

            };


            _processingThread = new Thread(processingThreadStart);
            _processingThread.Start();
            start.WaitOne();
        }

        private void RunWorkers()
        {
            while (_workerSemaphore.WaitOne())
            {
                ICrawlTask crawlTask;
                _workingQueue.TryDequeue(out crawlTask);
                if (crawlTask != null)
                {
                    UriFetcher fetcher = new UriFetcher(crawlTask);
                    fetcher.Fetch().ContinueWith(data =>
                    {
                        if (data.IsCompleted)
                        {
                            WorkDone(data.Result.CompletedTask);
                        }
                        _workerSemaphore.Release(1);
                    });
                }
                else
                {
                    _processWork.Reset();
                    _workerSemaphore.Release(1);
                    break;
                }

            }
        }

        private void FetchCrawlTasks(object state)
        {
            if (_workingQueue.Count < _options.LimitBeforeNewFetch)
            {
                if (!_isFetching)
                {
                    var fetchTask = _queue.FetchAsync(_options.MaxAmountToFetch);
                    _isFetching = true;
                    fetchTask.ContinueWith(taskFetched =>
                    {
                        foreach (var item in taskFetched.Result)
                        {
                            _workingQueue.Enqueue(item);
                        }
                        _isFetching = false;
                    });
                }
            }

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


        public async Task<WebDocument> CrawlAsync(ICrawlTask task)
        {
            _workingQueue.Enqueue(task);
            _processWork.Set();
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
            _fetchCrawlTaskTimer.Dispose();
            _waitHandle.Set();
            _processWork.Set();

            lock (_syncLock)
            {
                IsProcessing = false;
            }
            _processingThread.Join();
            _fetchCrawlTaskTimer.Dispose();


        }
    }

}
