// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Hermes.Example;
using Microsoft.JavaScript.NodeApi;
using static Microsoft.JavaScript.NodeApi.JSNativeApi.Interop;

HermesApi.Load("hermes.dll");
using var runtime = new HermesRuntime();
using var scope = new JSValueScope(JSValueScopeType.Root, (napi_env)runtime);

JSNativeApi.RunScript("x = 2");
Console.WriteLine($"Result: {(int)JSValue.Global["x"]}");
