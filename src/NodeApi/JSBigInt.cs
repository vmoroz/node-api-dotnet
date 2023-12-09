// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Microsoft.JavaScript.NodeApi.Runtime;
using static Microsoft.JavaScript.NodeApi.Runtime.JSRuntime;

namespace Microsoft.JavaScript.NodeApi;

public partial struct JSBigInt : IEquatable<JSBigInt>
{
    private readonly JSValue _value;

    public static implicit operator JSValue(JSBigInt value) => value._value;
    public static explicit operator JSBigInt?(JSValue value) => value.AsJSBigInt();
    public static explicit operator JSBigInt(JSValue value)
        => value.AsJSBigInt() is JSBigInt result
            ? result
            : throw new InvalidCastException("JSValue is not BigInt");

    public static implicit operator JSBigInt(BigInteger value) => new(value);
    public static explicit operator BigInteger(JSBigInt value) => value.ToBigInteger();

    private JSBigInt(JSValue value)
    {
        _value = value;
    }

    public JSBigInt(long value) : this(JSValue.CreateBigInt(value))
    {
    }

    public JSBigInt(ulong value) : this(JSValue.CreateBigInt(value))
    {
    }

    public JSBigInt(int sign, ReadOnlySpan<ulong> words)
        : this(JSValue.CreateBigInt(sign, words))
    {
    }

    public JSBigInt(BigInteger value) : this(JSValue.CreateBigInt(value))
    {
    }

    public static JSBigInt CreateUnchecked(JSValue value) => new JSBigInt(value);

    public JSValue AsJSValue() => _value;

    public BigInteger ToBigInteger() => _value.ToBigInteger();

    public long ToInt64(out bool isLossless)
        => _value.Runtime.GetValueBigInt64(
            _value.UncheckedEnvironmentHandle,
            (napi_value)_value,
            out long result,
            out isLossless)
            .ThrowIfFailed(result);

    public ulong ToUInt64(out bool isLossless)
        => _value.Runtime.GetValueBigInt64(
            _value.UncheckedEnvironmentHandle,
            (napi_value)_value,
            out ulong result,
            out isLossless)
            .ThrowIfFailed(result);

    public unsafe ulong[] ToUInt64Array(out int signBit)
    {
        napi_value nValue = (napi_value)_value;
        JSRuntime runtime = _value.Runtime;
        napi_env env = (napi_env)_value.Scope;
        runtime.GetValueBigInt(env, nValue, out _, new Span<ulong>(), out nuint wordCount)
            .ThrowIfFailed();
        ulong[] words = new ulong[wordCount];
        runtime.GetValueBigInt(env, nValue, out signBit, words.AsSpan(), out _)
            .ThrowIfFailed();
        return words;
    }

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSBigInt left, JSBigInt right)
        => left._value.StrictEquals(right._value);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSBigInt left, JSBigInt right)
        => !left._value.StrictEquals(right._value);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSBigInt other) => _value.StrictEquals(other._value);

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is JSBigInt other && Equals(other)
        || obj is JSValue otherValue && _value.StrictEquals(otherValue);

    /// <inheritdoc/>
    public override int GetHashCode()
        => throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
}
