// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.JavaScript.NodeApi.JSPromise;

namespace Hermes.Example;

public delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);

public sealed class DispatcherQueueShutdownStartingEventArgs
{
    Func<Deferral> _getDeferral;

    internal DispatcherQueueShutdownStartingEventArgs(Func<Deferral> getDeferral)
        => _getDeferral = getDeferral;

    public Deferral GetDeferral() => _getDeferral();
}

public class DispatcherQueue
{
    private readonly object _queueMutex = new();
    private List<Action?> _writerQueue = new(); // Queue to add new items
    private List<Action?> _readerQueue = new(); // Queue to read items from
    private TaskCompletionSource? _onShutdownCompleted;
    private int _threadId;
    private int _deferralCount;
    private bool _isShutdownCompleted;

    [ThreadStatic]
    private static DispatcherQueue? s_currentQueue;

    public event TypedEventHandler<DispatcherQueue, object?>? ShutdownCompleted;
    public event TypedEventHandler<DispatcherQueue, DispatcherQueueShutdownStartingEventArgs>?
        ShutdownStarting;

    public bool HasThreadAccess => _threadId == Environment.CurrentManagedThreadId;

    public static DispatcherQueue? GetForCurrentThread() => s_currentQueue;

    public bool TryEnqueue(Action callback)
    {
        lock (_queueMutex)
        {
            if (_isShutdownCompleted)
            {
                return false;
            }

            _writerQueue.Add(callback);
            Monitor.PulseAll(_queueMutex);
        }

        return true;
    }

    private struct CurrentQueueHolder : IDisposable
    {
        private readonly DispatcherQueue? _previousCurrentQueue;

        public CurrentQueueHolder(DispatcherQueue queue)
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

    // Run the thread function.
    internal void Run()
    {
        using var currentQueueHolder = new CurrentQueueHolder(this);

        // Loop until the shutdown completion breaks out of it.
        while (true)
        {
            // Invoke tasks from reader queue outside of lock.
            // The reader queue is only accessible from this thread.
            for (int i = 0; i < _readerQueue.Count; i++)
            {
                _readerQueue[i]?.Invoke();
                _readerQueue[i] = null;
            }

            // All tasks are completed. Clear the queue.
            _readerQueue.Clear();

            // Under lock see if we have more tasks, complete shutdown, or start waiting.
            lock (_queueMutex)
            {
                // Swap reader and writer queues.
                (_readerQueue, _writerQueue) = (_writerQueue, _readerQueue);

                if (_readerQueue.Count > 0)
                {
                    // We have more work to do. Start the loop from the beginning.
                    continue;
                }

                if (_onShutdownCompleted != null && _deferralCount == 0)
                {
                    // Complete the shutdown: the shutdown is already started,
                    // there are no deferrals, and all work is completed.
                    _isShutdownCompleted = true;
                    break;
                }

                // Wait for more work to come.
                Monitor.Wait(_queueMutex);
            }
        }

        // Notify about the shutdown completion.
        ShutdownCompleted?.Invoke(this, null);
        _onShutdownCompleted.SetResult();
    }

    // Create new Deferral and increment deferral count.
    internal Deferral CreateDeferral()
    {
        lock (_queueMutex)
        {
            _deferralCount++;
        }

        return new Deferral(() =>
        {
            // Decrement deferral count upon deferral completion.
            TryEnqueue(() => _deferralCount--);
        });
    }

    internal void Shutdown(TaskCompletionSource completion)
    {
        // Try to start the shutdown process.
        bool isShutdownStarted = TryEnqueue(() =>
        {
            if (_onShutdownCompleted != null)
            {
                // The shutdown is already started. Subscribe to its completion.
                ShutdownCompleted += (_, _) => completion.SetResult();
                return;
            }

            // Start the shutdown process.
            _onShutdownCompleted = completion;
            ShutdownStarting?.Invoke(
                this, new DispatcherQueueShutdownStartingEventArgs(() => CreateDeferral()));
        });

        if (!isShutdownStarted)
        {
            // The shutdown was already completed.
            completion.SetResult();
        }
    }
}


public class DispatcherQueueController
{
    public DispatcherQueue DispatcherQueue { get; } = new();

    public static DispatcherQueueController CreateOnDedicatedThread()
    {
        var controller = new DispatcherQueueController();
        DispatcherQueue queue = controller.DispatcherQueue;
        var thread = new Thread(() => queue.Run());
        thread.Start();
        return controller;
    }

    public Task ShutdownQueueAsync()
    {
        var completion = new TaskCompletionSource();
        DispatcherQueue.Shutdown(completion);
        return completion.Task;
    }
}

public sealed class Deferral : IDisposable
{
    private bool _isDisposed;
    private Action _completionHandler;

    public Deferral(Action completionHandler) => _completionHandler = completionHandler;

    public void Complete() => Dispose();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Deferral()
    {
        Dispose(false);
    }

    private void Dispose(bool _)
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _completionHandler.Invoke();
    }
}
