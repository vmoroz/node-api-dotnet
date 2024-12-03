// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.JavaScript.NodeApi.Runtime;

using static JSRuntime;

public sealed class NodejsEmbeddingRuntime
{
    private node_embedding_runtime _runtime;

    public NodejsEmbeddingRuntime(node_embedding_runtime runtime)
    {
        _runtime = runtime;
    }
}

/// <summary>
/// Manages a Node.js platform instance, provided by `libnode`.
/// </summary>
/// <remarks>
/// Only one Node.js platform instance can be created per process. Once the platform is disposed,
/// another platform instance cannot be re-initialized. One or more <see cref="NodejsEnvironment" />
/// instances may be created using the platform.
/// </remarks>
public sealed class EmbeddedNodejs : IDisposable
{
    private node_embedding_platform _platform;

    public delegate node_embedding_status HandleErrorCallback(
        string[] messages, node_embedding_status status);
    public delegate node_embedding_status ConfigurePlatformCallback(
        node_embedding_platform_config platformConfig);
    public delegate node_embedding_status ConfigureRuntimeCallback(
        node_embedding_platform platform, node_embedding_runtime_config platformConfig);
    public delegate void GetArgsCallback(string[] args);
    public delegate void PreloadCallback(
        NodejsEmbeddingRuntime runtime, JSValue process, JSValue require);
    public delegate JSValue StartExecutionCallback(
        NodejsEmbeddingRuntime runtime, JSValue process, JSValue require, JSValue runCommonJS);
    public delegate void HandleResultCallback(
        NodejsEmbeddingRuntime runtime, JSValue value);
    public delegate JSValue InitializeModuleCallback(
        NodejsEmbeddingRuntime runtime, string moduleName, JSValue exports);
    public delegate void RunTaskCallback();
    public delegate void PostTaskCallback(node_embedding_run_task_functor runTask);
    public delegate void RunNodeApiCallback(NodejsEmbeddingRuntime runtime);

    public class Settings
    {
        public string[]? Args { get; set; }
        public node_embedding_platform_flags? PlatformFlags { get; set; }
        public HandleErrorCallback? OnError { get; set; }
    }

    //    public delegate void ConfigurePlatform();

    public static unsafe EmbeddedNodejs Initialize(string libnodePath, Settings? settings)
    {
        if (string.IsNullOrEmpty(libnodePath)) throw new ArgumentNullException(nameof(libnodePath));
        if (Current != null)
        {
            throw new InvalidOperationException(
                "Only one Node.js platform instance per process is allowed.");
        }
        nint libnodeHandle = NativeLibrary.Load(libnodePath);
        Runtime = new NodejsRuntime(libnodeHandle);

        if (settings != null && settings.OnError != null)
        {
            var handle_error_functor = new node_embedding_handle_error_functor
            {
                data = (nint)GCHandle.Alloc(settings.OnError),
                invoke = new node_embedding_handle_error_callback(s_handleErrorCallback),
                release = new node_embedding_release_data_callback(s_releaseDataCallback),
            };
            Runtime.EmbeddingOnError(handle_error_functor).ThrowIfFailed();
        }

        Runtime.EmbeddingSetApiVersion(
            1, // The intitial Node.js embedding API version
            9) // Current Node-API version
            .ThrowIfFailed();

        ConfigurePlatformCallback configurePlatform = (config) =>
         {
             if (settings != null && settings.PlatformFlags != null)
             {
                 Runtime.EmbeddingPlatformSetFlags(config, settings.PlatformFlags.Value);
             }
             return node_embedding_status.ok;
         };
        var configurePlatformFunctor = new node_embedding_configure_platform_functor_ref
        {
            data = (nint)GCHandle.Alloc(configurePlatform),
            invoke = new node_embedding_configure_platform_callback(s_configurePlatformCallback),
        };

        node_embedding_platform platform;
        try
        {
            Runtime.EmbeddingCreatePlatform(settings?.Args, configurePlatformFunctor, out platform)
                .ThrowIfFailed();
        }
        finally
        {
            GCHandle.FromIntPtr(configurePlatformFunctor.data).Free();
        }

        Current = new EmbeddedNodejs(platform);

        return Current;
    }

    private EmbeddedNodejs(node_embedding_platform platform)
    {
        _platform = platform;
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        Runtime?.EmbeddingDeletePlatform(_platform);
    }

    public bool IsDisposed { get; private set; }

    public static EmbeddedNodejs? Current { get; private set; }

    public static JSRuntime? Runtime { get; private set; }

    //    private readonly node_embedding_platform _platform;

    //    public static implicit operator node_embedding_platform(NodeEmbeddingPlatform platform)
    //        => platform._platform;

    //    public static void SetErrorHandler(HandleErrorCallback callback)
    //    {
    //        if (callback == null)
    //        {
    //            throw new ArgumentNullException(nameof(callback));
    //        }
    //        Runtime.EmbeddingOnError(s_handleErrorCallback, GCHandle.Alloc(callback).ToIntPtr())
    //            .ThrowIfFailed();
    //    }

    //    /// <summary>
    //    /// Initializes the Node.js platform.
    //    /// </summary>
    //    /// <param name="libnodePath">Path to the `libnode` shared library, including extension.</param>
    //    /// <param name="args">Optional platform arguments.</param>
    //    /// <exception cref="InvalidOperationException">A Node.js platform instance has already been
    //    /// loaded in the current process.</exception>
    //    public NodeEmbeddingPlatform(
    //        string libnodePath,
    //        string[]? args = null)
    //    {
    //        if (string.IsNullOrEmpty(libnodePath)) throw new ArgumentNullException(nameof(libnodePath));

    //        if (Current != null)
    //        {
    //            throw new InvalidOperationException(
    //                "Only one Node.js platform instance per process is allowed.");
    //        }

    //        nint libnodeHandle = NativeLibrary.Load(libnodePath);
    //        Runtime = new NodejsRuntime(libnodeHandle);

    //        Runtime.EmbeddingCreatePlatform(args, (error) => Console.WriteLine(error), out _platform)
    //            .ThrowIfFailed();
    //        Current = this;
    //    }

    //    /// <summary>
    //    /// Gets the Node.js platform instance for the current process, or null if not initialized.
    //    /// </summary>
    //    public static NodeEmbeddingPlatform? Current { get; private set; }

    //    public JSRuntime Runtime { get; }

    //    /// <summary>
    //    /// Gets a value indicating whether the current platform has been disposed.
    //    /// </summary>
    //    public bool IsDisposed { get; private set; }

    //    /// <summary>
    //    /// Disposes the platform. After disposal, another platform instance may not be initialized
    //    /// in the current process.
    //    /// </summary>
    //    public void Dispose()
    //    {
    //        if (IsDisposed) return;
    //        IsDisposed = true;

    //        Runtime.DestroyPlatform(_platform);
    //    }

    //    /// <summary>
    //    /// Creates a new Node.js environment with a dedicated main thread.
    //    /// </summary>
    //    /// <param name="baseDir">Optional directory that is used as the base directory when resolving
    //    /// imported modules, and also as the value of the global `__dirname` property. If unspecified,
    //    /// importing modules is not enabled and `__dirname` is undefined.</param>
    //    /// <param name="mainScript">Optional script to run in the environment. (Literal script content,
    //    /// not a path to a script file.)</param>
    //    /// <returns>A new <see cref="NodejsEnvironment" /> instance.</returns>
    //    public NodejsEnvironment CreateEnvironment(
    //        string? baseDir = null,
    //        string? mainScript = null)
    //    {
    //        if (IsDisposed) throw new ObjectDisposedException(nameof(NodeEmbeddingPlatform));

    //        return new NodejsEnvironment(this, baseDir, mainScript);
    //    }

#if !UNMANAGED_DELEGATES
    internal static readonly node_embedding_release_data_callback.Delegate
        s_releaseDataCallback = ReleaseDataCallbackAdapter;
    internal static readonly node_embedding_handle_error_callback.Delegate
        s_handleErrorCallback = HandleErrorCallbackAdapter;
    internal static readonly node_embedding_configure_platform_callback.Delegate
        s_configurePlatformCallback = ConfigurePlatformCallbackAdapter;
    internal static readonly node_embedding_configure_runtime_callback.Delegate
        s_configureRuntimeCallback = ConfigureRuntimeCallbackAdapter;
    internal static readonly node_embedding_get_args_callback.Delegate
        s_getArgsCallback = GetArgsCallbackAdapter;
    internal static readonly node_embedding_preload_callback.Delegate
        s_preloadCallback = PreloadCallbackAdapter;
    internal static readonly node_embedding_start_execution_callback.Delegate
        s_startExecutionCallback = StartExecutionCallbackAdapter;
    internal static readonly node_embedding_handle_result_callback.Delegate
        s_handleResultCallback = HandleResultCallbackAdapter;
    internal static readonly node_embedding_initialize_module_callback.Delegate
        s_initializeModuleCallback = InitializeModuleCallbackAdapter;
    internal static readonly node_embedding_run_task_callback.Delegate
        s_runTaskCallback = RunTaskCallbackAdapter;
    internal static readonly node_embedding_post_task_callback.Delegate
        s_postTaskCallback = PostTaskCallbackAdapter;
    internal static readonly node_embedding_run_node_api_callback.Delegate
        s_runNodeApiCallback = RunNodeApiCallbackAdapter;
#else
    internal static readonly unsafe delegate* unmanaged[Cdecl]<nint, void>
        s_releaseDataCallback = &ReleaseDataCallbackAdapter;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
        nint, nint, nuint, node_embedding_status, node_embedding_status>
        s_handleErrorCallback = &HandleErrorCallbackAdapter;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
        nint, node_embedding_platform_config, node_embedding_status>
        s_configurePlatformCallback = &ConfigurePlatformCallbackAdapter;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
        nint,
        node_embedding_platform,
        node_embedding_runtime_config,
        node_embedding_status>
        s_configureRuntimeCallback = &ConfigureRuntimeCallbackAdapter;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
        nint, int, nint, void>
        s_getArgsCallback = &GetArgsCallbackAdapter;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
        nint, node_embedding_runtime, napi_env, napi_value, napi_value, void>
        s_preloadCallback = &PreloadCallbackAdapter;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
        nint, node_embedding_runtime, napi_env, napi_value, napi_value, napi_value, napi_value>
        s_startExecutionCallback = &StartExecutionCallbackAdapter;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
        nint, node_embedding_runtime, napi_env, napi_value, void>
        s_handleResultCallback = &HandleResultCallbackAdapter;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
        nint, node_embedding_runtime, napi_env, nint, napi_value, napi_value>
        s_initializeModuleCallback = &InitializeModuleCallbackAdapter;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<nint, void>
        s_runTaskCallback = &RunTaskCallbackAdapter;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
        nint, node_embedding_run_task_functor, void>
        s_postTaskCallback = &PostTaskCallbackAdapter;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
        nint, node_embedding_runtime, napi_env, void>
        s_runNodeApiCallback = &RunNodeApiCallbackAdapter;
#endif

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void ReleaseDataCallbackAdapter(nint data)
    {
        if (data != default)
        {
            GCHandle.FromIntPtr(data).Free();
        }
    }


#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe node_embedding_status HandleErrorCallbackAdapter(
        nint cb_data,
        nint messages,
        nuint messages_size,
        node_embedding_status status)
    {
        var callback = (HandleErrorCallback)GCHandle.FromIntPtr(cb_data).Target!;
        return callback(Utf8StringArray.ToStringArray(messages, (int)messages_size), status);
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe node_embedding_status ConfigurePlatformCallbackAdapter(
            nint cb_data,
            node_embedding_platform_config platform_config)
    {
        var callback = (ConfigurePlatformCallback)GCHandle.FromIntPtr(cb_data).Target!;
        return callback(platform_config);
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe node_embedding_status ConfigureRuntimeCallbackAdapter(
            nint cb_data,
            node_embedding_platform platform,
            node_embedding_runtime_config runtime_config)
    {
        var callback = (ConfigureRuntimeCallback)GCHandle.FromIntPtr(cb_data).Target!;
        return callback(platform, runtime_config);
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void GetArgsCallbackAdapter(
            nint cb_data, int argc, nint argv)
    {
        var callback = (GetArgsCallback)GCHandle.FromIntPtr(cb_data).Target!;
        callback(Utf8StringArray.ToStringArray(argv, argc));
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void PreloadCallbackAdapter(
            nint cb_data,
            node_embedding_runtime runtime,
            napi_env env,
            napi_value process,
            napi_value require)
    {
        var callback = (PreloadCallback)GCHandle.FromIntPtr(cb_data).Target!;
        var embeddingRuntime = new NodejsEmbeddingRuntime(runtime);
        var JSValueScope = new JSValueScope(JSValueScopeType.Root, env, Runtime);
        callback(embeddingRuntime, new JSValue(process), new JSValue(require));
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe napi_value StartExecutionCallbackAdapter(
            nint cb_data,
            node_embedding_runtime runtime,
            napi_env env,
            napi_value process,
            napi_value require,
            napi_value run_cjs)
    {
        var callback = (StartExecutionCallback)GCHandle.FromIntPtr(cb_data).Target!;
        var embeddingRuntime = new NodejsEmbeddingRuntime(runtime);
        var JSValueScope = new JSValueScope(JSValueScopeType.Root, env, Runtime);
        return (napi_value)callback(
            embeddingRuntime, new JSValue(process), new JSValue(require), new JSValue(run_cjs));
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void HandleResultCallbackAdapter(
            nint cb_data,
            node_embedding_runtime runtime,
            napi_env env,
            napi_value value)
    {
        var callback = (HandleResultCallback)GCHandle.FromIntPtr(cb_data).Target!;
        var embeddingRuntime = new NodejsEmbeddingRuntime(runtime);
        var JSValueScope = new JSValueScope(JSValueScopeType.Root, env, Runtime);
        callback(embeddingRuntime, new JSValue(value));
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe napi_value InitializeModuleCallbackAdapter(
            nint cb_data,
            node_embedding_runtime runtime,
            napi_env env,
            nint module_name,
            napi_value exports)
    {
        var callback = (InitializeModuleCallback)GCHandle.FromIntPtr(cb_data).Target!;
        var embeddingRuntime = new NodejsEmbeddingRuntime(runtime);
        var JSValueScope = new JSValueScope(JSValueScopeType.Root, env, Runtime);
        return (napi_value)callback(
            embeddingRuntime,
            Utf8StringArray.PtrToStringUTF8((byte*)module_name),
            new JSValue(exports));
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void RunTaskCallbackAdapter(nint cb_data)
    {
        var callback = (RunTaskCallback)GCHandle.FromIntPtr(cb_data).Target!;
        callback();
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void PostTaskCallbackAdapter(
            nint cb_data,
            node_embedding_run_task_functor run_task)
    {
        var callback = (PostTaskCallback)GCHandle.FromIntPtr(cb_data).Target!;
        callback(run_task);
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void RunNodeApiCallbackAdapter(
            nint cb_data,
            node_embedding_runtime runtime,
            napi_env env)
    {
        var callback = (RunNodeApiCallback)GCHandle.FromIntPtr(cb_data).Target!;
        var embeddingRuntime = new NodejsEmbeddingRuntime(runtime);
        var JSValueScope = new JSValueScope(JSValueScopeType.Root, env, Runtime);
        callback(embeddingRuntime);
    }
}
