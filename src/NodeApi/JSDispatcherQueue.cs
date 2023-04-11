// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.NodeApi;

public sealed class DispatcherQueueShutdownStartingEventArgs : EventArgs
{
    private readonly Func<JSDispatcherQueueDeferral> _getDeferral;

    internal DispatcherQueueShutdownStartingEventArgs(Func<JSDispatcherQueueDeferral> getDeferral)
        => _getDeferral = getDeferral;

    public IDisposable GetDeferral() => _getDeferral();
}

public sealed class JSDispatcherQueue
{
    private readonly object _queueMutex = new();
    private List<Action> _writerQueue = new(); // The queue to add new actions.
    private List<Action> _readerQueue = new(); // The queue to read actions from.
    private readonly List<JSDispatcherQueueTimer.Job> _timerJobs = new();
    private TaskCompletionSource<int>? _onShutdownCompleted;
    private int _threadId;
    private int _deferralCount;
    private bool _isShutdownCompleted;

    [ThreadStatic]
    private static JSDispatcherQueue? s_currentQueue;

    public event EventHandler? ShutdownCompleted;
    public event EventHandler<DispatcherQueueShutdownStartingEventArgs>? ShutdownStarting;

    public bool HasThreadAccess => _threadId == Environment.CurrentManagedThreadId;

    public static JSDispatcherQueue? GetForCurrentThread() => s_currentQueue;

    public bool TryEnqueue(Action callback)
    {
        lock (_queueMutex)
        {
            return TryEnqueueInternal(callback);
        }
    }

    internal bool TryEnqueueInternal(Action callback)
    {
        ValidateLock();
        if (_isShutdownCompleted)
        {
            return false;
        }

        _writerQueue.Add(callback);
        Monitor.PulseAll(_queueMutex);
        return true;
    }

    public JSDispatcherQueueTimer CreateTimer()
        => new JSDispatcherQueueTimer(this);

    // Run the thread function.
    internal void Run()
    {
        using var currentQueueHolder = new CurrentQueueHolder(this);

        // Loop until the shutdown completion breaks out of it.
        while (true)
        {
            // Invoke tasks from reader queue outside of lock.
            // The reader queue is only accessible from this thread.
            foreach (Action task in _readerQueue)
            {
                task();
            }

            // All tasks are completed. Clear the queue.
            _readerQueue.Clear();

            // Under lock see if we have more tasks, complete shutdown, or start waiting.
            lock (_queueMutex)
            {
                // See if must run timers
                DateTime now = DateTime.Now;
                TimeSpan waitTimeout = Timeout.InfiniteTimeSpan;
                for (int i = _timerJobs.Count - 1; i >= 0; i--)
                {
                    JSDispatcherQueueTimer.Job timerJob = _timerJobs[i];
                    if (now >= timerJob.TickTime)
                    {
                        _timerJobs.RemoveAt(i);
                        _readerQueue.Add(timerJob.Invoke);
                    }
                    else
                    {
                        // The wait timeout for the next timer run activation.
                        waitTimeout = timerJob.TickTime - now;
                        break;
                    }
                }

                if (_readerQueue.Count == 0)
                {
                    // Swap reader and writer queues.
                    (_readerQueue, _writerQueue) = (_writerQueue, _readerQueue);
                }

                if (_readerQueue.Count > 0)
                {
                    // We have more work to do. Start the loop from the beginning.
                    continue;
                }

                if (IsShutdownStarted && _deferralCount == 0)
                {
                    // Complete the shutdown: the shutdown is already started,
                    // there are no deferrals, and all work is completed.
                    _isShutdownCompleted = true;
                    break;
                }

                // Wait for more work to come.
                Monitor.Wait(_queueMutex, waitTimeout);
            }
        }

        // Notify about the shutdown completion.
        ShutdownCompleted?.Invoke(this, EventArgs.Empty);
        _onShutdownCompleted?.SetResult(0);
    }

    // Create new Deferral and increment deferral count.
    internal JSDispatcherQueueDeferral CreateDeferral()
    {
        lock (_queueMutex)
        {
            _deferralCount++;
        }

        return new JSDispatcherQueueDeferral(() =>
        {
            // Decrement deferral count upon deferral completion.
            TryEnqueue(() =>
            {
                lock (_queueMutex)
                {
                    _deferralCount--;
                }
            });
        });
    }

    internal bool IsShutdownStarted => _onShutdownCompleted != null;

    internal void Shutdown(TaskCompletionSource<int> completion)
    {
        // Try to start the shutdown process.
        bool isShutdownEnqueued = TryEnqueue(() =>
        {
            if (IsShutdownStarted)
            {
                // The shutdown is already started. Subscribe to its completion.
                ShutdownCompleted += (_, _) => completion.SetResult(0);
                return;
            }

            StartShutdown();
        });

        if (!isShutdownEnqueued)
        {
            // The shutdown was already completed.
            completion.SetResult(0);
        }

        void StartShutdown()
        {
            _onShutdownCompleted = completion;
            ShutdownStarting?.Invoke(
                this, new DispatcherQueueShutdownStartingEventArgs(() => CreateDeferral()));
        }
    }

    private readonly struct CurrentQueueHolder : IDisposable
    {
        private readonly JSDispatcherQueue? _previousCurrentQueue;

        public CurrentQueueHolder(JSDispatcherQueue queue)
        {
            _previousCurrentQueue = s_currentQueue;
            s_currentQueue = queue;
            queue._threadId = Environment.CurrentManagedThreadId;
        }

        public void Dispose()
        {
            if (s_currentQueue != null)
            {
                s_currentQueue._threadId = default;
            }

            s_currentQueue = _previousCurrentQueue;
        }
    }

    internal void AddTimerJob(JSDispatcherQueueTimer.Job timerJob)
    {
        ValidateNoLock();
        if (timerJob.IsCancelled) return;

        // See if we can invoke it immediately.
        if (timerJob.TickTime <= DateTime.Now)
        {
            timerJob.Invoke();
            return;
        }

        lock (_queueMutex)
        {
            // Schedule for future invocation.
            int index = _timerJobs.BinarySearch(timerJob);
            // If the index negative, then it is a bitwise complement of
            // the suggested insertion index.
            if (index < 0) index = ~index;
            _timerJobs.Insert(index, timerJob);
        }
    }

    internal void InvokeUnderLock(Action action)
    {
        lock (_queueMutex)
        {
            action();
        }
    }

    internal void ValidateLock()
    {
        if (!Monitor.IsEntered(_queueMutex))
        {
            throw new InvalidOperationException("_queueMutex must be locked");
        }
    }

    internal void ValidateNoLock()
    {
        if (Monitor.IsEntered(_queueMutex))
        {
            throw new InvalidOperationException("_queueMutex must not be locked");
        }
    }
}

public class JSDispatcherQueueController
{
    public JSDispatcherQueue DispatcherQueue { get; } = new();

    public static JSDispatcherQueueController CreateOnDedicatedThread()
    {
        var controller = new JSDispatcherQueueController();
        JSDispatcherQueue queue = controller.DispatcherQueue;
        var thread = new Thread(() => queue.Run());
        thread.Start();
        return controller;
    }

    public Task ShutdownQueueAsync()
    {
        var completion = new TaskCompletionSource<int>();
        DispatcherQueue.Shutdown(completion);
        return completion.Task;
    }
}

public sealed class JSDispatcherQueueTimer
{
    private readonly JSDispatcherQueue _queue;
    private TimeSpan _interval;
    private bool _isRepeating = true;
    private Job? _currentJob;

    public TimeSpan Interval
    {
        get => _interval;
        set
        {
            _queue.InvokeUnderLock(() =>
            {
                if (_interval == value) return;
                _interval = value;
                RestartInternal();
            });
        }
    }

    public bool IsRepeating
    {
        get => _isRepeating;
        set
        {
            _queue.InvokeUnderLock(() =>
            {
                if (_isRepeating == value) return;
                _isRepeating = value;
                RestartInternal();
            });
        }
    }

    public bool IsRunning => _currentJob != null;

    public event EventHandler? Tick;

    public JSDispatcherQueueTimer(JSDispatcherQueue queue) => _queue = queue;

    public void Start() => _queue.InvokeUnderLock(StartInternal);

    public void Stop() => _queue.InvokeUnderLock(StopInternal);

    private void StartInternal()
    {
        _queue.ValidateLock();
        if (_currentJob != null) return;
        if (Tick == null) return;

        var timerJob = new Job(this, DateTime.Now + Interval, Tick);
        if (_queue.TryEnqueueInternal(() => _queue.AddTimerJob(timerJob)))
        {
            _currentJob = timerJob;
        }
    }

    private void StopInternal()
    {
        _queue.ValidateLock();
        if (_currentJob == null) return;

        _currentJob.Cancel();
        _currentJob = null;
    }

    private void RestartInternal()
    {
        if (_currentJob == null) return;
        StopInternal();
        StartInternal();
    }

    private void CompleteJob(Job job)
    {
        _queue.InvokeUnderLock(() =>
        {
            if (_currentJob == job)
            {
                _currentJob = null;
            }

            if (IsRepeating)
            {
                StartInternal();
            }
        });
    }

    internal class Job : IComparable<Job>
    {
        public JSDispatcherQueueTimer Timer { get; }
        public DateTime TickTime { get; }
        public EventHandler Tick { get; }
        public bool IsCancelled { get; private set; }

        public Job(JSDispatcherQueueTimer timer, DateTime tickTime, EventHandler tick)
        {
            Timer = timer;
            TickTime = tickTime;
            Tick = tick;
        }

        public int CompareTo(Job? other)
        {
            if (other == null) return 1;
            // Sort in descending order where the timer runs with lower timer
            // appear in the end of the list. It is to optimize deletion from the run list.
            return -Comparer<DateTime>.Default.Compare(TickTime, other.TickTime);
        }

        public void Cancel() => IsCancelled = true;

        public void Invoke()
        {
            if (IsCancelled) return;
            Tick?.Invoke(Timer, EventArgs.Empty);
            Timer.CompleteJob(this);
        }
    }
}

internal sealed class JSDispatcherQueueDeferral : IDisposable
{
    private bool _isDisposed;
    private readonly Action _completionHandler;

    public JSDispatcherQueueDeferral(Action completionHandler)
        => _completionHandler = completionHandler;

    ~JSDispatcherQueueDeferral()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool _)
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _completionHandler.Invoke();
    }
}
