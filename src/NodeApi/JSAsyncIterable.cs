// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.JavaScript.NodeApi;

public readonly ref partial struct JSAsyncIterable
{
    private readonly JSValue _value;

    public static explicit operator JSAsyncIterable(JSValue value) => new(value);
    public static implicit operator JSValue(JSAsyncIterable iterable) => iterable._value;

    public static explicit operator JSAsyncIterable(JSObject obj) => (JSAsyncIterable)(JSValue)obj;
    public static implicit operator JSObject(JSAsyncIterable iterable) => (JSObject)iterable._value;

    private JSAsyncIterable(JSValue value)
    {
        _value = value;
    }

#pragma warning disable IDE0060 // Unused parameter
    public Enumerator GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new(_value);
#pragma warning restore IDE0060

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSAsyncIterable a, JSAsyncIterable b)
        => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSAsyncIterable a, JSAsyncIterable b)
        => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}
