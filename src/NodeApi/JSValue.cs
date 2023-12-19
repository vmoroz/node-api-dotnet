// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.JavaScript.NodeApi.Interop;
using Microsoft.JavaScript.NodeApi.Runtime;
using static Microsoft.JavaScript.NodeApi.JSNativeApi;
using static Microsoft.JavaScript.NodeApi.JSValueScope;
using static Microsoft.JavaScript.NodeApi.Runtime.JSRuntime;

namespace Microsoft.JavaScript.NodeApi;

public readonly struct JSValue : IEquatable<JSValue>
{
    private readonly napi_value _handle = default;
    private readonly JSValueScope? _scope = null;

    public readonly JSValueScope Scope => _scope ?? JSValueScope.Current;

    internal JSRuntime Runtime => Scope.Runtime;

    /// <summary>
    /// Creates an empty instance of <see cref="JSValue" />, which implicitly converts to
    /// <see cref="JSValue.Undefined" /> when used in any scope.
    /// </summary>
    public JSValue()
    {
        _handle = default;
        _scope = null;
    }

    /// <summary>
    /// Creates a new instance of <see cref="JSValue" /> from a handle in the current scope.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the handle is null.</exception>
    /// <remarks>
    /// WARNING: A JS value handle is a pointer to a location in memory, so an invalid handle here
    /// may cause an attempt to access an invalid memory location.
    /// </remarks>
    public JSValue(napi_value handle) : this(handle, Current) { }

    /// <summary>
    /// Creates a new instance of <see cref="JSValue" /> from a handle in the specified scope.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the handle is null</exception>
    /// <remarks>
    /// WARNING: A JS value handle is a pointer to a location in memory, so an invalid handle here
    /// may cause an attempt to access an invalid memory location.
    /// </remarks>
    public JSValue(napi_value handle, JSValueScope scope)
    {
        if (handle.IsNull) throw new ArgumentNullException(nameof(handle));
        _handle = handle;
        _scope = scope;
    }

    /// <summary>
    /// Gets the value handle, or throws an exception if the value scope is disposed or
    /// access from the current thread is invalid.
    /// </summary>
    /// <exception cref="JSValueScopeClosedException">The scope has been closed.</exception>
    /// <exception cref="JSInvalidThreadAccessException">The scope is not valid on the current
    /// thread.</exception>
    public napi_value Handle
    {
        get
        {
            if (_scope == null)
            {
                // If the scope is null, this is an empty (uninitialized) instance.
                // Implicitly convert to the JS `undefined` value.
                return GetCurrentRuntime(out napi_env env)
                    .GetUndefined(env, out napi_value result).ThrowIfFailed(result);
            }

            // Ensure the scope is valid and on the current thread (environment).
            _scope.ThrowIfDisposed();
            _scope.ThrowIfInvalidThreadAccess();

            // The handle must be non-null when the scope is non-null.
            return _handle;
        }
    }

    public static implicit operator JSValue(napi_value handle) => new(handle);
    public static implicit operator JSValue?(napi_value handle) => handle.Handle != default ? new(handle) : default;
    public static explicit operator napi_value(JSValue value) => value.Handle;
    public static explicit operator napi_value(JSValue? value) => value?.Handle ?? default;

    /// <summary>
    /// Gets the environment handle for the value's scope without checking whether the scope
    /// is disposed or whether access from the current thread is valid. WARNING: This must only
    /// be used to avoid redundant handle checks when there is another (checked) access to
    /// <see cref="Handle" /> for the same call.
    /// </summary>
    internal napi_env UncheckedEnvironmentHandle => Scope.UncheckedEnvironmentHandle;

    public static JSValue Undefined => default;
    public static JSValue Null => GetCurrentRuntime(out napi_env env)
        .GetNull(env, out napi_value result).ThrowIfFailed(result);
    public static JSValue Global => GetCurrentRuntime(out napi_env env)
        .GetGlobal(env, out napi_value result).ThrowIfFailed(result);
    public static JSValue True => GetBoolean(true);
    public static JSValue False => GetBoolean(false);
    public static JSValue GetBoolean(bool value) => GetCurrentRuntime(out napi_env env)
        .GetBoolean(env, value, out napi_value result).ThrowIfFailed(result);

    public JSObject Properties => (JSObject)this;

    public JSArray Items => (JSArray)this;

    public JSValue this[JSValue name]
    {
        get => this.GetProperty(name);
        set => this.SetProperty(name, value);
    }

    public JSValue this[string name]
    {
        get => this.GetProperty(name);
        set => this.SetProperty(name, value);
    }

    public JSValue this[int index]
    {
        get => this.GetElement(index);
        set => this.SetElement(index, value);
    }

    public static JSValue CreateObject() => GetCurrentRuntime(out napi_env env)
        .CreateObject(env, out napi_value result).ThrowIfFailed(result);

    public static JSValue CreateArray() => GetCurrentRuntime(out napi_env env)
        .CreateArray(env, out napi_value result).ThrowIfFailed(result);

    public static JSValue CreateArray(int length) => GetCurrentRuntime(out napi_env env)
        .CreateArray(env, length, out napi_value result).ThrowIfFailed(result);

    public static JSValue CreateNumber(double value) => GetCurrentRuntime(out napi_env env)
        .CreateNumber(env, value, out napi_value result).ThrowIfFailed(result);

    public static JSValue CreateNumber(int value) => GetCurrentRuntime(out napi_env env)
        .CreateNumber(env, value, out napi_value result).ThrowIfFailed(result);

    public static JSValue CreateNumber(uint value) => GetCurrentRuntime(out napi_env env)
        .CreateNumber(env, value, out napi_value result).ThrowIfFailed(result);

    public static JSValue CreateNumber(long value) => GetCurrentRuntime(out napi_env env)
        .CreateNumber(env, value, out napi_value result).ThrowIfFailed(result);

    public static unsafe JSValue CreateStringUtf8(ReadOnlySpan<byte> value)
    {
        fixed (byte* spanPtr = value)
        {
            return GetCurrentRuntime(out napi_env env)
                .CreateString(env, value, out napi_value result).ThrowIfFailed(result);
        }
    }

    public static unsafe JSValue CreateStringUtf16(ReadOnlySpan<char> value)
    {
        fixed (char* spanPtr = value)
        {
            return GetCurrentRuntime(out napi_env env)
                .CreateString(env, value, out napi_value result).ThrowIfFailed(result);
        }
    }

    public static unsafe JSValue CreateStringUtf16(string value)
    {
        fixed (char* spanPtr = value)
        {
            return GetCurrentRuntime(out napi_env env)
                .CreateString(env, value.AsSpan(), out napi_value result).ThrowIfFailed(result);
        }
    }

    public static JSValue CreateSymbol(JSValue description) => GetCurrentRuntime(out napi_env env)
        .CreateSymbol(env, (napi_value)description, out napi_value result).ThrowIfFailed(result);

    public static JSValue SymbolFor(string name) => GetCurrentRuntime(out napi_env env)
        .GetSymbolFor(env, name, out napi_value result).ThrowIfFailed(result);

    public static JSValue CreateFunction(
        string? name,
        napi_callback callback,
        nint data)
    {
        return GetCurrentRuntime(out napi_env env)
            .CreateFunction(env, name, callback, data, out napi_value result).ThrowIfFailed(result);
    }

    public static unsafe JSValue CreateFunction(
        string? name, JSCallback callback, object? callbackData = null)
    {
        GCHandle descriptorHandle = JSRuntimeContext.Current.AllocGCHandle(
            new JSCallbackDescriptor(name, callback, callbackData));
        JSValue func = CreateFunction(
            name,
            new napi_callback(
                JSValueScope.Current?.ScopeType == JSValueScopeType.NoContext ?
                s_invokeJSCallbackNC : s_invokeJSCallback),
            (nint)descriptorHandle);
        func.AddGCHandleFinalizer((nint)descriptorHandle);
        return func;
    }

    public static JSValue CreateError(JSValue? code, JSValue message)
        => GetCurrentRuntime(out napi_env env)
            .CreateError(env, (napi_value)code, (napi_value)message, out napi_value result)
            .ThrowIfFailed(result);

    public static JSValue CreateTypeError(JSValue? code, JSValue message)
        => GetCurrentRuntime(out napi_env env)
            .CreateTypeError(env, (napi_value)code, (napi_value)message, out napi_value result)
            .ThrowIfFailed(result);

    public static JSValue CreateRangeError(JSValue? code, JSValue message)
        => GetCurrentRuntime(out napi_env env)
            .CreateRangeError(env, (napi_value)code, (napi_value)message, out napi_value result)
            .ThrowIfFailed(result);

    public static JSValue CreateSyntaxError(JSValue? code, JSValue message)
        => GetCurrentRuntime(out napi_env env)
            .CreateSyntaxError(env, (napi_value)code, (napi_value)message, out napi_value result)
        .ThrowIfFailed(result);

    public static unsafe JSValue CreateExternal(object value)
    {
        JSValueScope currentScope = JSValueScope.Current;
        GCHandle valueHandle = currentScope.RuntimeContext.AllocGCHandle(value);
        return currentScope.Runtime.CreateExternal(
            currentScope.UncheckedEnvironmentHandle,
            (nint)valueHandle,
            new napi_finalize(s_finalizeGCHandle),
            currentScope.RuntimeContextHandle,
            out napi_value result)
            .ThrowIfFailed(result);
    }

    public static JSValue CreateArrayBuffer(int byteLength)
        => GetCurrentRuntime(out napi_env env)
            .CreateArrayBuffer(env, byteLength, out nint _, out napi_value result)
            .ThrowIfFailed(result);

    public static unsafe JSValue CreateArrayBuffer(ReadOnlySpan<byte> data)
    {
        GetCurrentRuntime(out napi_env env)
            .CreateArrayBuffer(env, data.Length, out nint buffer, out napi_value result)
            .ThrowIfFailed();
        data.CopyTo(new Span<byte>((void*)buffer, data.Length));
        return result;
    }

    public static unsafe JSValue CreateExternalArrayBuffer<T>(
        Memory<T> memory, object? external = null) where T : struct
    {
        var pinnedMemory = new PinnedMemory<T>(memory, external);
        return GetCurrentRuntime(out napi_env env).CreateArrayBuffer(
            env,
            (nint)pinnedMemory.Pointer,
            pinnedMemory.Length,
            // We pass object to finalize as a hint parameter
            new napi_finalize(s_finalizeGCHandleToPinnedMemory),
            (nint)pinnedMemory.RuntimeContext.AllocGCHandle(pinnedMemory),
            out napi_value result)
            .ThrowIfFailed(result);
    }

    public static JSValue CreateDataView(int length, JSValue arrayBuffer, int byteOffset)
        => GetCurrentRuntime(out napi_env env)
            .CreateDataView(env, length, (napi_value)arrayBuffer, byteOffset, out napi_value result)
            .ThrowIfFailed(result);

    public static JSValue CreateTypedArray(
        JSTypedArrayType type, int length, JSValue arrayBuffer, int byteOffset)
        => GetCurrentRuntime(out napi_env env).CreateTypedArray(
            env,
            (napi_typedarray_type)type,
            length,
            (napi_value)arrayBuffer,
            byteOffset,
            out napi_value result)
            .ThrowIfFailed(result);

    public static JSValue CreatePromise(out JSPromise.Deferred deferred)
    {
        GetCurrentRuntime(out napi_env env)
            .CreatePromise(env, out napi_deferred deferred_, out napi_value promise)
            .ThrowIfFailed();
        deferred = new JSPromise.Deferred(deferred_);
        return promise;
    }

    public static JSValue CreateDate(double time) => GetCurrentRuntime(out napi_env env)
        .CreateDate(env, time, out napi_value result).ThrowIfFailed(result);

    public static JSValue CreateBigInt(long value) => GetCurrentRuntime(out napi_env env)
        .CreateBigInt(env, value, out napi_value result).ThrowIfFailed(result);

    public static JSValue CreateBigInt(ulong value) => GetCurrentRuntime(out napi_env env)
        .CreateBigInt(env, value, out napi_value result).ThrowIfFailed(result);

    public static JSValue CreateBigInt(int signBit, ReadOnlySpan<ulong> words)
        => GetCurrentRuntime(out napi_env env)
            .CreateBigInt(env, signBit, words, out napi_value result).ThrowIfFailed(result);

    public JSValueType TypeOf() => _handle.IsNull
        ? JSValueType.Undefined
        : GetRuntime(out napi_env env).GetValueType(env, _handle, out napi_valuetype result)
            .ThrowIfFailed((JSValueType)result);

    public bool IsUndefined() => TypeOf() == JSValueType.Undefined;

    public bool IsNull() => TypeOf() == JSValueType.Null;

    public bool IsNullOrUndefined() => TypeOf() switch
    {
        JSValueType.Null => true,
        JSValueType.Undefined => true,
        _ => false,
    };

    public bool IsBoolean() => TypeOf() == JSValueType.Boolean;

    public bool IsNumber() => TypeOf() == JSValueType.Number;

    public bool IsString() => TypeOf() == JSValueType.String;

    public bool IsSymbol() => TypeOf() == JSValueType.Symbol;

    public bool IsObject() => TypeOf() switch
    {
        JSValueType.Object => true,
        JSValueType.Function => true,
        _ => false,
    };

    public bool IsFunction() => TypeOf() == JSValueType.Function;

    public bool IsExternal() => TypeOf() == JSValueType.External;

    public bool IsBigInt() => TypeOf() == JSValueType.BigInt;

    public double GetValueDouble() => GetRuntime(out napi_env env, out napi_value handle)
        .GetValueDouble(env, handle, out double result).ThrowIfFailed(result);

    public int GetValueInt32() => GetRuntime(out napi_env env, out napi_value handle)
        .GetValueInt32(env, handle, out int result).ThrowIfFailed(result);

    public uint GetValueUInt32() => GetRuntime(out napi_env env, out napi_value handle)
        .GetValueUInt32(env, handle, out uint result).ThrowIfFailed(result);

    public long GetValueInt64() => GetRuntime(out napi_env env, out napi_value handle)
        .GetValueInt64(env, handle, out long result).ThrowIfFailed(result);

    public bool GetValueBool() => GetRuntime(out napi_env env, out napi_value handle)
        .GetValueBool(env, handle, out bool result).ThrowIfFailed(result);

    public int GetValueStringUtf8(Span<byte> buffer)
        => GetRuntime(out napi_env env, out napi_value handle)
            .GetValueStringUtf8(env, handle, buffer, out int result)
            .ThrowIfFailed(result);

    public byte[] GetValueStringUtf8()
    {
        JSRuntime runtime = GetRuntime(out napi_env env, out napi_value handle);
        runtime.GetValueStringUtf8(env, handle, [], out int length).ThrowIfFailed();
        byte[] result = new byte[length + 1];
        runtime.GetValueStringUtf8(env, handle, new Span<byte>(result), out _).ThrowIfFailed();
        // Remove the zero terminating character
        Array.Resize(ref result, length);
        return result;
    }

    public unsafe int GetValueStringUtf16(Span<char> buffer)
        => GetRuntime(out napi_env env, out napi_value handle)
            .GetValueStringUtf16(env, handle, buffer, out int result)
            .ThrowIfFailed(result);

    public char[] GetValueStringUtf16AsCharArray()
    {
        JSRuntime runtime = GetRuntime(out napi_env env, out napi_value handle);
        runtime.GetValueStringUtf16(env, handle, [], out int length).ThrowIfFailed();
        char[] result = new char[length + 1];
        runtime.GetValueStringUtf16(env, handle, new Span<char>(result), out _).ThrowIfFailed();
        // Remove the zero terminating character
        Array.Resize(ref result, length);
        return result;
    }

    //TODO: (vmoroz) improve
    public string GetValueStringUtf16() => new(GetValueStringUtf16AsCharArray());

    public JSValue CoerceToBoolean() => GetRuntime(out napi_env env, out napi_value handle)
        .CoerceToBool(env, handle, out napi_value result).ThrowIfFailed(result);

    public JSValue CoerceToNumber() => GetRuntime(out napi_env env, out napi_value handle)
        .CoerceToNumber(env, handle, out napi_value result).ThrowIfFailed(result);

    public JSValue CoerceToObject() => GetRuntime(out napi_env env, out napi_value handle)
        .CoerceToObject(env, handle, out napi_value result).ThrowIfFailed(result);

    public JSValue CoerceToString() => GetRuntime(out napi_env env, out napi_value handle)
        .CoerceToString(env, handle, out napi_value result).ThrowIfFailed(result);

    public JSValue GetPrototype() => GetRuntime(out napi_env env, out napi_value handle)
        .GetPrototype(env, handle, out napi_value result).ThrowIfFailed(result);

    public static implicit operator JSValue(bool value) => GetBoolean(value);
    public static implicit operator JSValue(sbyte value) => CreateNumber(value);
    public static implicit operator JSValue(byte value) => CreateNumber(value);
    public static implicit operator JSValue(short value) => CreateNumber(value);
    public static implicit operator JSValue(ushort value) => CreateNumber(value);
    public static implicit operator JSValue(int value) => CreateNumber(value);
    public static implicit operator JSValue(uint value) => CreateNumber(value);
    public static implicit operator JSValue(long value) => CreateNumber(value);
    public static implicit operator JSValue(ulong value) => CreateNumber(value);
    public static implicit operator JSValue(float value) => CreateNumber(value);
    public static implicit operator JSValue(double value) => CreateNumber(value);
    public static implicit operator JSValue(bool? value) => ValueOrDefault(value, value => GetBoolean(value));
    public static implicit operator JSValue(sbyte? value) => ValueOrDefault(value, value => CreateNumber(value));
    public static implicit operator JSValue(byte? value) => ValueOrDefault(value, value => CreateNumber(value));
    public static implicit operator JSValue(short? value) => ValueOrDefault(value, value => CreateNumber(value));
    public static implicit operator JSValue(ushort? value) => ValueOrDefault(value, value => CreateNumber(value));
    public static implicit operator JSValue(int? value) => ValueOrDefault(value, value => CreateNumber(value));
    public static implicit operator JSValue(uint? value) => ValueOrDefault(value, value => CreateNumber(value));
    public static implicit operator JSValue(long? value) => ValueOrDefault(value, value => CreateNumber(value));
    public static implicit operator JSValue(ulong? value) => ValueOrDefault(value, value => CreateNumber(value));
    public static implicit operator JSValue(float? value) => ValueOrDefault(value, value => CreateNumber(value));
    public static implicit operator JSValue(double? value) => ValueOrDefault(value, value => CreateNumber(value));
    public static implicit operator JSValue(string value) => value == null ? default : CreateStringUtf16(value);
    public static implicit operator JSValue(char[] value) => value == null ? default : CreateStringUtf16(value);
    public static implicit operator JSValue(Span<char> value) => CreateStringUtf16(value);
    public static implicit operator JSValue(ReadOnlySpan<char> value) => CreateStringUtf16(value);
    public static implicit operator JSValue(byte[] value) => value == null ? default : CreateStringUtf8(value);
    public static implicit operator JSValue(Span<byte> value) => CreateStringUtf8(value);
    public static implicit operator JSValue(ReadOnlySpan<byte> value) => CreateStringUtf8(value);

    public static explicit operator bool(JSValue value) => value.GetValueBool();
    public static explicit operator sbyte(JSValue value) => (sbyte)value.GetValueInt32();
    public static explicit operator byte(JSValue value) => (byte)value.GetValueUInt32();
    public static explicit operator short(JSValue value) => (short)value.GetValueInt32();
    public static explicit operator ushort(JSValue value) => (ushort)value.GetValueUInt32();
    public static explicit operator int(JSValue value) => value.GetValueInt32();
    public static explicit operator uint(JSValue value) => value.GetValueUInt32();
    public static explicit operator long(JSValue value) => value.GetValueInt64();
    public static explicit operator ulong(JSValue value) => (ulong)value.GetValueInt64();
    public static explicit operator float(JSValue value) => (float)value.GetValueDouble();
    public static explicit operator double(JSValue value) => value.GetValueDouble();
    public static explicit operator string(JSValue value) => value.IsNullOrUndefined() ? null! : value.GetValueStringUtf16();
    public static explicit operator char[](JSValue value) => value.IsNullOrUndefined() ? null! : value.GetValueStringUtf16AsCharArray();
    public static explicit operator byte[](JSValue value) => value.IsNullOrUndefined() ? null! : value.GetValueStringUtf8();
    public static explicit operator bool?(JSValue value) => ValueOrDefault(value, value => value.GetValueBool());
    public static explicit operator sbyte?(JSValue value) => ValueOrDefault(value, value => (sbyte)value.GetValueInt32());
    public static explicit operator byte?(JSValue value) => ValueOrDefault(value, value => (byte)value.GetValueUInt32());
    public static explicit operator short?(JSValue value) => ValueOrDefault(value, value => (short)value.GetValueInt32());
    public static explicit operator ushort?(JSValue value) => ValueOrDefault(value, value => (ushort)value.GetValueUInt32());
    public static explicit operator int?(JSValue value) => ValueOrDefault(value, value => value.GetValueInt32());
    public static explicit operator uint?(JSValue value) => ValueOrDefault(value, value => value.GetValueUInt32());
    public static explicit operator long?(JSValue value) => ValueOrDefault(value, value => value.GetValueInt64());
    public static explicit operator ulong?(JSValue value) => ValueOrDefault(value, value => (ulong)value.GetValueInt64());
    public static explicit operator float?(JSValue value) => ValueOrDefault(value, value => (float)value.GetValueDouble());
    public static explicit operator double?(JSValue value) => ValueOrDefault(value, value => value.GetValueDouble());

    private static JSValue ValueOrDefault<T>(T? value, Func<T, JSValue> convert) where T : struct
        => value.HasValue ? convert(value.Value) : default;

    private static T? ValueOrDefault<T>(JSValue value, Func<JSValue, T> convert) where T : struct
        => value.IsNullOrUndefined() ? default : convert(value);

    /// <summary>
    /// Delegate that provides a conversion from some type to a JS value.
    /// </summary>
    public delegate JSValue From<T>(T value);

    /// <summary>
    /// Delegate that provides a conversion from a JS value to some type.
    /// </summary>
    public delegate T To<T>(JSValue value);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSValue a, JSValue b) => a.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSValue a, JSValue b) => !a.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => this.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSValue other && Equals(other);
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }

    private JSRuntime GetRuntime(out napi_env env)
    {
        JSValueScope scope = _scope ?? throw new ArgumentNullException(nameof(scope));
        scope.ThrowIfDisposed();
        scope.ThrowIfInvalidThreadAccess();
        env = scope.UncheckedEnvironmentHandle;
        return scope.Runtime;
    }

    private JSRuntime GetRuntime(out napi_env env, out napi_value handle)
    {
        if (_scope is JSValueScope scope)
        {
            scope.ThrowIfDisposed();
            scope.ThrowIfInvalidThreadAccess();
            env = scope.UncheckedEnvironmentHandle;
            handle = _handle;
            return scope.Runtime;
        }
        else
        {
            scope = Current;
            env = scope.UncheckedEnvironmentHandle;
            JSRuntime runtime = scope.Runtime;
            runtime.GetUndefined(env, out handle).ThrowIfFailed();
            return runtime;
        }
    }

    private static JSRuntime GetCurrentRuntime(out napi_env env)
    {
        JSValueScope scope = Current;
        env = scope.UncheckedEnvironmentHandle;
        return scope.Runtime;
    }
}
