// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.JavaScript.NodeApi.Runtime;

using static JSRuntime;

/// <summary>
/// Manages a Node.js platform instance, provided by `libnode`.
/// </summary>
/// <remarks>
/// Only one Node.js platform instance can be created per process. Once the platform is disposed,
/// another platform instance cannot be re-initialized. One or more <see cref="NodejsEnvironment" />
/// instances may be created using the platform.
/// </remarks>
public sealed class NodeEmbeddingPlatform : IDisposable
{
    private readonly node_embedding_platform _platform;

    public static implicit operator node_embedding_platform(NodeEmbeddingPlatform platform)
        => platform._platform;

    public static void SetErrorHandler(HandleErrorCallback callback)
    {
        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback));
        }
        Runtime.EmbeddingOnError(s_handleErrorCallback, GCHandle.Alloc(callback).ToIntPtr())
            .ThrowIfFailed();
    }

    /// <summary>
    /// Initializes the Node.js platform.
    /// </summary>
    /// <param name="libnodePath">Path to the `libnode` shared library, including extension.</param>
    /// <param name="args">Optional platform arguments.</param>
    /// <exception cref="InvalidOperationException">A Node.js platform instance has already been
    /// loaded in the current process.</exception>
    public NodeEmbeddingPlatform(
        string libnodePath,
        string[]? args = null)
    {
        if (string.IsNullOrEmpty(libnodePath)) throw new ArgumentNullException(nameof(libnodePath));

        if (Current != null)
        {
            throw new InvalidOperationException(
                "Only one Node.js platform instance per process is allowed.");
        }

        nint libnodeHandle = NativeLibrary.Load(libnodePath);
        Runtime = new NodejsRuntime(libnodeHandle);

        Runtime.EmbeddingCreatePlatform(args, (error) => Console.WriteLine(error), out _platform)
            .ThrowIfFailed();
        Current = this;
    }

    /// <summary>
    /// Gets the Node.js platform instance for the current process, or null if not initialized.
    /// </summary>
    public static NodeEmbeddingPlatform? Current { get; private set; }

    public JSRuntime Runtime { get; }

    /// <summary>
    /// Gets a value indicating whether the current platform has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Disposes the platform. After disposal, another platform instance may not be initialized
    /// in the current process.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;

        Runtime.DestroyPlatform(_platform);
    }

    /// <summary>
    /// Creates a new Node.js environment with a dedicated main thread.
    /// </summary>
    /// <param name="baseDir">Optional directory that is used as the base directory when resolving
    /// imported modules, and also as the value of the global `__dirname` property. If unspecified,
    /// importing modules is not enabled and `__dirname` is undefined.</param>
    /// <param name="mainScript">Optional script to run in the environment. (Literal script content,
    /// not a path to a script file.)</param>
    /// <returns>A new <see cref="NodejsEnvironment" /> instance.</returns>
    public NodejsEnvironment CreateEnvironment(
        string? baseDir = null,
        string? mainScript = null)
    {
        if (IsDisposed) throw new ObjectDisposedException(nameof(NodeEmbeddingPlatform));

        return new NodejsEnvironment(this, baseDir, mainScript);
    }

#if !UNMANAGED_DELEGATES
    internal static readonly node_embedding_release_data_callback.Delegate
        s_releaseDataCallback = ReleaseDataCallback;
    internal static readonly node_embedding_handle_error_callback.Delegate
        s_handleErrorCallback = AdaptHandleErrorCallback;
    internal static readonly node_embedding_configure_platform_callback.Delegate
        s_configurePlatformCallback = ConfigurePlatformCallback;
    internal static readonly node_embedding_configure_runtime_callback.Delegate
        s_configureRuntimeCallback = ConfigureRuntimeCallback;
    internal static readonly node_embedding_get_args_callback.Delegate
        s_getArgsCallback = GetArgsCallback;
    internal static readonly node_embedding_preload_callback.Delegate
        s_preloadCallback = PreloadCallback;
    internal static readonly node_embedding_start_execution_callback.Delegate
        s_startExecutionCallback = StartExecutionCallback;
    internal static readonly node_embedding_handle_result_callback.Delegate
        s_handleResultCallback = HandleResultCallback;
    internal static readonly node_embedding_initialize_module_callback.Delegate
        s_initializeModuleCallback = InitializeModuleCallback;
    internal static readonly node_embedding_run_task_callback.Delegate
        s_runTaskCallback = RunTaskCallback;
    internal static readonly node_embedding_post_task_callback.Delegate
        s_postTaskCallback = PostTaskCallback;
    internal static readonly node_embedding_run_node_api_callback.Delegate
        s_runNodeApiCallback = RunNodeApiCallback;
#else
    internal static readonly unsafe delegate* unmanaged[Cdecl]<nint, void>
        s_releaseDataCallback = &ReleaseDataCallback;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
            nint, nint, nuint, node_embedding_status, node_embedding_status>
        s_handleErrorCallback = &AdaptHandleErrorCallback;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
            nint, node_embedding_platform_config, node_embedding_status>
        s_configurePlatformCallback = &ConfigurePlatformCallback;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
            nint,
            node_embedding_platform,
            node_embedding_runtime_config,
            node_embedding_status>
        s_configureRuntimeCallback = &ConfigureRuntimeCallback;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
            nint, int, nint, node_embedding_status>
        s_getArgsCallback = &GetArgsCallback;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
            nint, node_embedding_runtime, napi_env, napi_value, napi_value, void>
        s_preloadCallback = &PreloadCallback;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
            nint, node_embedding_runtime, napi_env, napi_value, napi_value, napi_value, napi_value>
        s_startExecutionCallback = &StartExecutionCallback;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
            nint, node_embedding_runtime, napi_env, napi_value, void>
        s_handleResultCallback = &HandleResultCallback;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
            nint, node_embedding_runtime, napi_env, nint, napi_value, napi_value>
        s_initializeModuleCallback = &InitializeModuleCallback;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<nint, void>
        s_runTaskCallback = &RunTaskCallback;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
            nint, node_embedding_run_task_functor, void>
        s_postTaskCallback = &PostTaskCallback;
    internal static readonly unsafe delegate* unmanaged[Cdecl]<
            nint, node_embedding_runtime, napi_env, void>
        s_runNodeApiCallback = &RunNodeApiCallback;
#endif

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void ReleaseDataCallback(nint data)
    {
        if (data != default)
        {
            GCHandle.FromIntPtr(data).Free();
        }
    }

    public delegate node_embedding_status HandleErrorCallback(
        string[] messages, node_embedding_status status);


#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe node_embedding_status AdaptHandleErrorCallback(
        nint cb_data,
        nint messages,
        nuint messages_size,
        node_embedding_status status)
    {
        var callabck = (HandleErrorCallback)GCHandle.FromIntPtr(cb_data).Target!;
        return callabck(Utf8StringArray.ToStringArray(messages, (int)messages_size), status);
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe node_embedding_status ConfigurePlatformCallback(
            nint cb_data,
            node_embedding_platform_config platform_config)
    {
    }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe node_embedding_status ConfigureRuntimeCallback(
            nint cb_data,
            node_embedding_platform platform,
            node_embedding_runtime_config runtime_config)
    { }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe node_embedding_status GetArgsCallback(
            nint cb_data, int argc, nint argv)
    { }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void PreloadCallback(
            nint cb_data,
            node_embedding_runtime runtime,
            napi_env env,
            napi_value process,
            napi_value require)
    { }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe napi_value StartExecutionCallback(
            nint cb_data,
            node_embedding_runtime runtime,
            napi_env env,
            napi_value process,
            napi_value require,
            napi_value run_cjs)
    { }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void HandleResultCallback(
            nint cb_data,
            node_embedding_runtime runtime,
            napi_env env,
            napi_value value)
    { }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe napi_value InitializeModuleCallback(
            nint cb_data,
            node_embedding_runtime runtime,
            napi_env env,
            nint module_name,
            napi_value exports)
    { }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void RunTaskCallback(nint cb_data) { }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void PostTaskCallback(
            nint cb_data,
            node_embedding_run_task_functor run_task)
    { }

#if UNMANAGED_DELEGATES
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    internal static unsafe void RunNodeApiCallback(
            nint cb_data,
            node_embedding_runtime runtime,
            napi_env env)
    {
        if (cb_data == default || runtime == default || env == default) return;

    }
}
