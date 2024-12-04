// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.JavaScript.NodeApi.Runtime;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static JSRuntime;
using static NodejsEmbedding;

/// <summary>
/// A Node.js runtime.
/// </summary>
/// <remarks>
/// Multiple Node.js environments may be created (concurrently) in the same process.
/// </remarks>
public sealed class NodejsEmbeddingRuntime : IDisposable
{
    private node_embedding_runtime _runtime;
    private static readonly
        Dictionary<node_embedding_runtime, NodejsEmbeddingRuntime> _embeddedRuntimes = new();

    public static implicit operator node_embedding_runtime(NodejsEmbeddingRuntime runtime)
        => runtime._runtime;

    public struct Module
    {
        public string Name { get; set; }
        public InitializeModuleCallback OnInitialize { get; set; }
        public int? NodeApiVersion { get; set; }
    }

    public class RuntimeSettings
    {
        public node_embedding_runtime_flags? RuntimeFlags { get; set; }
        public string[]? Args { get; set; }
        public string[]? RuntimeArgs { get; set; }
        public PreloadCallback? OnPreload { get; set; }
        public StartExecutionCallback? StartExecution { get; set; }
        public HandleResultCallback? HandleStartExecutionResult { get; set; }
        public IEnumerable<Module>? Modules { get; set; }
        public PostTaskCallback? OnPostTask { get; set; }
        public ConfigureRuntimeCallback? ConfigureRuntime { get; set; }

        public static unsafe implicit operator node_embedding_configure_runtime_functor_ref(
            RuntimeSettings? settings)
        {
            var confgureRuntime = new ConfigureRuntimeCallback((platform, config) =>
            {
                if (settings?.RuntimeFlags != null)
                {
                    JSRuntime.EmbeddingRuntimeSetFlags(config, settings.RuntimeFlags.Value)
                        .ThrowIfFailed();
                }
                if (settings?.Args != null || settings?.RuntimeArgs != null)
                {
                    JSRuntime.EmbeddingRuntimeSetArgs(config, settings.Args, settings.RuntimeArgs)
                        .ThrowIfFailed();
                }
                if (settings?.OnPreload != null)
                {
                    var preloadFunctor = new node_embedding_preload_functor
                    {
                        data = (nint)GCHandle.Alloc(settings.OnPreload),
                        invoke = new node_embedding_preload_callback(s_preloadCallback),
                        release = new node_embedding_release_data_callback(s_releaseDataCallback),
                    };
                    JSRuntime.EmbeddingRuntimeOnPreload(config, preloadFunctor).ThrowIfFailed();
                }
                if (settings?.StartExecution != null)
                {
                    var startExecutionFunctor = new node_embedding_start_execution_functor
                    {
                        data = (nint)GCHandle.Alloc(settings.StartExecution),
                        invoke = new node_embedding_start_execution_callback(
                            s_startExecutionCallback),
                        release = new node_embedding_release_data_callback(s_releaseDataCallback),
                    };
                    var handleStartExecutionResultFunctor = new node_embedding_handle_result_functor
                    {
                        data = settings.HandleStartExecutionResult != null
                            ? (nint)GCHandle.Alloc(settings.HandleStartExecutionResult) : 0,
                        invoke = settings.HandleStartExecutionResult != null
                            ? new node_embedding_handle_result_callback(s_handleResultCallback)
                            : new node_embedding_handle_result_callback(0),
                        release = settings.HandleStartExecutionResult != null
                            ? new node_embedding_release_data_callback(s_releaseDataCallback)
                            : new node_embedding_release_data_callback(0),
                    };
                    JSRuntime.EmbeddingRuntimeOnStartExecution(
                        config, startExecutionFunctor, handleStartExecutionResultFunctor)
                        .ThrowIfFailed();
                }
                if (settings?.Modules != null)
                {
                    foreach (Module module in settings.Modules)
                    {
                        var moduleFunctor = new node_embedding_initialize_module_functor
                        {
                            data = (nint)GCHandle.Alloc(module.OnInitialize),
                            invoke = new node_embedding_initialize_module_callback(
                                s_initializeModuleCallback),
                            release = new node_embedding_release_data_callback(
                                s_releaseDataCallback),
                        };

                        JSRuntime.EmbeddingRuntimeAddModule(
                            config,
                            module.Name,
                            moduleFunctor,
                            module.NodeApiVersion ?? NodeApiVersion)
                            .ThrowIfFailed();
                    }
                }
                if (settings?.OnPostTask != null)
                {
                    var postTaskFunctor = new node_embedding_post_task_functor
                    {
                        data = (nint)GCHandle.Alloc(settings.OnPostTask),
                        invoke = new node_embedding_post_task_callback(s_postTaskCallback),
                        release = new node_embedding_release_data_callback(s_releaseDataCallback),
                    };
                    JSRuntime.EmbeddingRuntimeSetTaskRunner(config, postTaskFunctor).ThrowIfFailed();
                }
                settings?.ConfigureRuntime?.Invoke(platform, config);
            });

            return new node_embedding_configure_runtime_functor_ref(
                confgureRuntime,
                new node_embedding_configure_runtime_callback(s_configureRuntimeCallback));
        }
    }

    public static JSRuntime JSRuntime => NodejsEmbedding.JSRuntime;

    public NodejsEmbeddingRuntime(
        NodejsEmbeddingPlatform platform, RuntimeSettings? settings = null)
    {
        JSRuntime.EmbeddingCreateRuntime(platform, settings ?? new RuntimeSettings(), out _runtime)
            .ThrowIfFailed();
    }

    private NodejsEmbeddingRuntime(node_embedding_runtime runtime)
    {
        _runtime = runtime;
        lock (_embeddedRuntimes) { _embeddedRuntimes.Add(runtime, this); }
    }

    public static NodejsEmbeddingRuntime? FromHandle(node_embedding_runtime runtime)
    {
        lock (_embeddedRuntimes)
        {
            if (_embeddedRuntimes.TryGetValue(
                runtime, out NodejsEmbeddingRuntime? embeddingRuntime))
            {
                return embeddingRuntime;
            }
            return null;
        }
    }

    public static void Run(NodejsEmbeddingPlatform platform, RuntimeSettings? settings)
    {
        JSRuntime.EmbeddingRunRuntime(platform, settings ?? new RuntimeSettings()).ThrowIfFailed();
    }

    /// <summary>
    /// Gets a value indicating whether the Node.js environment is disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Disposes the Node.js environment, causing its main thread to exit.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;

        lock (_embeddedRuntimes) { _embeddedRuntimes.Remove(_runtime); }
        JSRuntime.EmbeddingDeleteRuntime(_runtime).ThrowIfFailed();
    }

    public unsafe bool RunEventLoop(node_embedding_event_loop_run_mode runMode)
    {
        if (IsDisposed) throw new ObjectDisposedException(nameof(NodejsEmbeddingRuntime));

        return JSRuntime.EmbeddingRunEventLoop(_runtime, runMode, out bool hasMoreWork)
            .ThrowIfFailed(hasMoreWork);
    }

    public unsafe void CompleteEventLoop()
    {
        if (IsDisposed) throw new ObjectDisposedException(nameof(NodejsEmbeddingRuntime));

        JSRuntime.EmbeddingCompleteEventLoop(_runtime).ThrowIfFailed();
    }

    public unsafe void TerminateEventLoop()
    {
        if (IsDisposed) throw new ObjectDisposedException(nameof(NodejsEmbeddingRuntime));

        JSRuntime.EmbeddingTerminateEventLoop(_runtime).ThrowIfFailed();
    }

    public unsafe void RunNodeApi(RunNodeApiCallback runNodeApi)
    {
        if (IsDisposed) throw new ObjectDisposedException(nameof(NodejsEmbeddingRuntime));

        using var runNodeApiFunctorRef = new node_embedding_run_node_api_functor_ref(
            runNodeApi, new node_embedding_run_node_api_callback(s_runNodeApiCallback));
        JSRuntime.EmbeddingRunNodeApi(_runtime, runNodeApiFunctorRef).ThrowIfFailed();
    }
}
