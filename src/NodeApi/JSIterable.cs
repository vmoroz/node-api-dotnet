// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.JavaScript.NodeApi;

public readonly ref partial struct JSIterable
{
    private readonly JSValue _value;

    public static explicit operator JSIterable(JSValue value) => new(value);
    public static implicit operator JSValue(JSIterable iterable) => iterable._value;

    public static explicit operator JSArray(JSIterable iterable) => (JSArray)iterable._value;
    public static implicit operator JSIterable(JSArray array) => (JSIterable)(JSValue)array;

    public static explicit operator JSIterable(JSObject obj) => (JSIterable)(JSValue)obj;
    public static implicit operator JSObject(JSIterable iterable) => (JSObject)iterable._value;

    private JSIterable(JSValue value)
    {
        _value = value;
    }

    public Enumerator GetEnumerator() => new(_value);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSIterable a, JSIterable b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSIterable a, JSIterable b) => !a._value.StrictEquals(b);

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
