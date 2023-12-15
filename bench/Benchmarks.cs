// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Microsoft.JavaScript.NodeApi.DotNetHost;
using Microsoft.JavaScript.NodeApi.Interop;
using Microsoft.JavaScript.NodeApi.Runtime;
using static Microsoft.JavaScript.NodeApi.Runtime.JSRuntime;
using static Microsoft.JavaScript.NodeApi.Test.TestUtils;

namespace Microsoft.JavaScript.NodeApi.Bench;

/// <summary>
/// Micro-benchmarks for various .NET + JS interop operations.
/// </summary>
/// <remarks>
/// These benchmarks run both .NET and Node.js code, and call between them. The benchmark
/// runner manages the GC for the .NET runtime, but it doesn't know anything about the JS runtime.
/// To avoid heavy JS GC pressure from millions of operations (which may each allocate objects),
/// these benchmarks use the `ShortRunJob` attribute (which sacrifices some precision but also
/// doesn't take as long to run).
/// </remarks>
[IterationCount(5)]
[WarmupCount(1)]
public abstract class Benchmarks
{
    public static void Main(string[] args)
    {
#if DEBUG
        IConfig config = new DebugBuildConfig();
#else
        IConfig config = DefaultConfig.Instance;
#endif

        // Example: dotnet run -c Release --filter clr
        // If no filter is specified, the switcher will prompt.
        BenchmarkSwitcher.FromAssembly(typeof(Benchmarks).Assembly).Run(args,
            ManualConfig.Create(config)
            .WithOptions(ConfigOptions.JoinSummary));
    }

    private static string LibnodePath { get; } = Path.Combine(
        GetRepoRootDirectory(),
        "bin",
        GetCurrentPlatformRuntimeIdentifier(),
        "libnode" + GetSharedLibraryExtension());

    private napi_env _env;
    private JSValueChecked _jsFunction;
    private JSValueChecked _jsFunctionWithArgs;
    private JSValueChecked _jsFunctionWithCallback;
    private JSValueChecked _jsInstance;
    private JSValueChecked _dotnetFunction;
    private JSValueChecked _dotnetFunctionWithArgs;
    private JSValueChecked _dotnetClass;
    private JSValueChecked _dotnetInstance;
    private JSValueChecked _jsFunctionCreateInstance;
    private JSValueChecked _jsFunctionCallMethod;
    private JSValueChecked _jsFunctionCallMethodWithArgs;
    private JSReference _reference = null!;

    /// <summary>
    /// Simple class that is exported to JS and used in some benchmarks.
    /// </summary>
    private class DotnetClass
    {
        public DotnetClass() { }

        public string Property { get; set; } = string.Empty;

#pragma warning disable CA1822 // Method does not access instance data and can be marked as static
        public static void Method() { }
#pragma warning restore CA1822
    }

    /// <summary>
    /// Setup shared by both CLR and AOT benchmarks.
    /// </summary>
    protected void Setup()
    {
        NodejsPlatform platform = new(LibnodePath/*, args: new[] { "node", "--expose-gc" }*/);

        // This setup avoids using NodejsEnvironment so benchmarks can run on the same thread.
        // NodejsEnvironment creates a separate thread that would slow down the micro-benchmarks.
        platform.Runtime.CreateEnvironment(platform, Console.WriteLine, null, out _env)
            .ThrowIfFailed();

        // The new scope instance saves itself as the thread-local JSValueScope.Current.
        JSValueScope scope = new(JSValueScopeType.Root, _env, platform.Runtime);

        // Create some JS values that will be used by the benchmarks.

        _jsFunction = JSNativeApi.RunScript("function jsFunction() { }; jsFunction");
        _jsFunctionWithArgs = JSNativeApi.RunScript(
            "function jsFunctionWithArgs(a, b, c) { }; jsFunctionWithArgs");
        _jsFunctionWithCallback = JSNativeApi.RunScript(
            "function jsFunctionWithCallback(cb, ...args) { cb(...args); }; " +
            "jsFunctionWithCallback");
        _jsInstance = JSNativeApi.RunScript(
            "const jsInstance = { method: (...args) => {} }; jsInstance");

        _dotnetFunction = JSValue.CreateFunction(
            "dotnetFunction", (args) => JSValue.Undefined);
        _dotnetFunctionWithArgs = JSValue.CreateFunction(
            "dotnetFunctionWithArgs", (args) =>
            {
                for (int i = 0; i < args.Length; i++)
                {
                    _ = args[i];
                }

                return JSValue.Undefined;
            });

        var classBuilder = new JSClassBuilder<DotnetClass>(
            nameof(DotnetClass), () => new DotnetClass());
        classBuilder.AddProperty(
            "property",
            (DotnetClass x) => (JSValueChecked)(JSValue)x.Property,
            (DotnetClass x, JSValueChecked value) => x.Property = (string)(JSValue)value);
        classBuilder.AddMethod("method", (x) => (args) => DotnetClass.Method());
        _dotnetClass = classBuilder.DefineClass(JSValue.Undefined);
        _dotnetInstance = JSNativeApi.CallAsConstructor((JSValue)_dotnetClass);

        _jsFunctionCreateInstance = JSNativeApi.RunScript(
            "function jsFunctionCreateInstance(Class) { new Class() }; " +
            "jsFunctionCreateInstance");
        _jsFunctionCallMethod = JSNativeApi.RunScript(
            "function jsFunctionCallMethod(instance) { instance.method(); }; " +
            "jsFunctionCallMethod");
        _jsFunctionCallMethodWithArgs = JSNativeApi.RunScript(
            "function jsFunctionCallMethodWithArgs(instance, ...args) " +
            "{ instance.method(...args); }; " +
            "jsFunctionCallMethodWithArgs");

        _reference = new JSReference(_jsFunction.Value);
    }

    private static JSValueScope NewJSScope() => new(JSValueScopeType.Callback);

    // Benchmarks in the base class run in both CLR and AOT environments.

    [Benchmark]
    public void CallJSFunction()
    {
        _jsFunction.AsFunction().CallAsStatic();
    }

    [Benchmark]
    public void CallJSFunctionWithArgs()
    {
        _jsFunctionWithArgs.AsFunction().CallAsStatic("1", "2", "3");
    }

    [Benchmark]
    public void CallJSMethod()
    {
        _jsInstance.AsObject().CallMethod("method");
    }

    [Benchmark]
    public void CallJSMethodWithArgs()
    {
        _jsInstance.AsObject().CallMethod("method", "1", "2", "3");
    }

    [Benchmark]
    public void CallDotnetFunction()
    {
        _jsFunctionWithCallback.AsFunction().CallAsStatic(_dotnetFunction);
    }

    [Benchmark]
    public void CallDotnetFunctionWithArgs()
    {
        _jsFunctionWithCallback.AsFunction().CallAsStatic(_dotnetFunctionWithArgs, "1", "2", "3");
    }

    [Benchmark]
    public void CallDotnetConstructor()
    {
        _jsFunctionCreateInstance.AsFunction().CallAsStatic(_dotnetClass);
    }

    [Benchmark]
    public void CallDotnetMethod()
    {
        _jsFunctionCallMethod.AsFunction().CallAsStatic(_dotnetInstance);
    }

    [Benchmark]
    public void CallDotnetMethodWithArgs()
    {
        _jsFunctionCallMethodWithArgs.AsFunction().CallAsStatic(
            _dotnetInstance.Value, "1", "2", "3");
    }

    [Benchmark]
    public void ReferenceGet()
    {
        _ = _reference.GetValue();
    }

    [Benchmark]
    public void ReferenceCreateAndDispose()
    {
        using JSReference reference = new(_jsFunction.Value);
    }

    [ShortRunJob]
    [MemoryDiagnoser(displayGenColumns: false)]
    public class Clr : Benchmarks
    {
        private JSValueChecked _jsHost;
        private JSValueChecked _jsFunctionCallMethodDynamic;
        private JSValueChecked _jsFunctionCallMethodDynamicInterface;

        [GlobalSetup]
        public new void Setup()
        {
            base.Setup();

            // CLR-only (non-AOT) setup

            JSObject hostModule = new();
            _ = new ManagedHost(hostModule);
            _jsHost = (JSValue)hostModule;
            _jsFunctionCallMethodDynamic = JSNativeApi.RunScript(
                "function jsFunctionCallMethodDynamic(dotnet) " +
                "{ dotnet.System.Object.ReferenceEquals(null, null); }; " +
                "jsFunctionCallMethodDynamic");

            // Implement IFormatProvider in JS and pass it to a .NET method.
            _jsFunctionCallMethodDynamicInterface = JSNativeApi.RunScript(
                "function jsFunctionCallMethodDynamicInterface(dotnet)  {" +
                "    const formatProvider = { GetFormat: (type) => null };" +
                "    dotnet.System.String.Format(formatProvider, '', null, null);" +
                "}; " +
                "jsFunctionCallMethodDynamicInterface");
        }

        // CLR-only (non-AOT) benchmarks

        [Benchmark]
        public void DynamicCallDotnetMethod()
        {
            _jsFunctionCallMethodDynamic.AsFunction().CallAsStatic(_jsHost);
        }

        [Benchmark]
        public void DynamicCallDotnetMethodWithInterface()
        {
            _jsFunctionCallMethodDynamicInterface.AsFunction().CallAsStatic(_jsHost);
        }
    }

    [ShortRunJob(RuntimeMoniker.NativeAot80)]
    public class Aot : Benchmarks
    {
        [GlobalSetup]
        public new void Setup()
        {
            base.Setup();
        }

        // AOT-only benchmarks
    }
}
