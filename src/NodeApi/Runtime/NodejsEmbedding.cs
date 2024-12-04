// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.JavaScript.NodeApi.Runtime;

using System;
#if UNMANAGED_DELEGATES
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;
using static JSRuntime;

public sealed class NodejsEmbedding
{
    public static readonly int EmbeddingApiVersion = 1;
    public static readonly int NodeApiVersion = 9;

    public static JSRuntime JSRuntime
    {
        get
        {
            if (NodejsEmbeddingPlatform.JSRuntime == null)
            {
                throw new InvalidOperationException("The JSRuntime is not initialized.");
            }
            return NodejsEmbeddingPlatform.JSRuntime;
        }
    }

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

#if UNMANAGED_DELEGATES
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
#else
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
        var embeddingRuntime = NodejsEmbeddingRuntime.FromHandle(runtime);
        var JSValueScope = new JSValueScope(JSValueScopeType.Root, env, JSRuntime);
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
        var embeddingRuntime = NodejsEmbeddingRuntime.FromHandle(runtime);
        var JSValueScope = new JSValueScope(JSValueScopeType.Root, env, JSRuntime);
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
        var embeddingRuntime = NodejsEmbeddingRuntime.FromHandle(runtime);
        var JSValueScope = new JSValueScope(JSValueScopeType.Root, env, JSRuntime);
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
        var embeddingRuntime = NodejsEmbeddingRuntime.FromHandle(runtime);
        var JSValueScope = new JSValueScope(JSValueScopeType.Root, env, JSRuntime);
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
        var embeddingRuntime = NodejsEmbeddingRuntime.FromHandle(runtime);
        var JSValueScope = new JSValueScope(JSValueScopeType.Root, env, JSRuntime);
        callback(embeddingRuntime);
    }
}
