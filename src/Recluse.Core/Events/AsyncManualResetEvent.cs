using System.Threading.Tasks;

namespace Recluse.Core.Events
{
    public class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public Task WaitAsync() { return _tcs.Task; }

        public void Set() { _tcs.TrySetResult(true); }

        
    }
}
