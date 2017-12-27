using System.Threading;
using System.Threading.Tasks;

namespace NetworkCommunication
{
    public class StoppableTask
    {
        public StoppableTask(Task task, CancellationTokenSource cancellationTokenSource)
        {
            Task = task;
            CancellationTokenSource = cancellationTokenSource;
        }

        public Task Task { get; }
        public CancellationTokenSource CancellationTokenSource { get; }

        public void Stop()
        {
            CancellationTokenSource?.Cancel();
            Task?.Wait();
        }
    }
}