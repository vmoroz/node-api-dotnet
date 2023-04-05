// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using Hermes.Example;
using Microsoft.JavaScript.NodeApi;
using static Hermes.Example.HermesInterop;
using static Microsoft.JavaScript.NodeApi.JSNativeApi.Interop;

nint hermesLib = NativeLibrary.Load("hermes.dll");
HermesInterop.Initialize(hermesLib);
JSNativeApi.Interop.Initialize(hermesLib);

hermes_create_config(out hermes_config config);
hermes_create_runtime(config, out hermes_runtime runtime);
hermes_get_node_api_env(runtime, out napi_env env);

{
    using var scope = new JSValueScope(JSValueScopeType.Root, env);
    JSNativeApi.RunScript("x = 2");
    Console.WriteLine($"Result: {(int)JSValue.Global["x"]}");
}

Console.WriteLine("Hello, World!");

hermes_delete_runtime(runtime);
hermes_delete_config(config);
