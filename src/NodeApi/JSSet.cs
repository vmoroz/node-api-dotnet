// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.JavaScript.NodeApi.Interop;

namespace Microsoft.JavaScript.NodeApi;

public readonly ref partial struct JSSet
{
    private readonly JSValue _value;

    public static explicit operator JSSet(JSValue value) => new(value);
    public static implicit operator JSValue(JSSet set) => set._value;

    public static explicit operator JSSet(JSObject obj) => (JSSet)(JSValue)obj;
    public static implicit operator JSObject(JSSet set) => (JSObject)set._value;

    public static explicit operator JSSet(JSIterable obj) => (JSSet)(JSValue)obj;
    public static implicit operator JSIterable(JSSet set) => (JSIterable)set._value;


    private JSSet(JSValue value)
    {
        _value = value;
    }

    /// <summary>
    /// Creates a new empty JS Set.
    /// </summary>
    public JSSet()
    {
        _value = JSRuntimeContext.Current.Import(null, "Set").CallAsConstructor();
    }

    /// <summary>
    /// Creates a new JS Set with values from an iterable (such as another set).
    /// </summary>
    public JSSet(JSIterable iterable)
    {
        _value = JSRuntimeContext.Current.Import(null, "Set").CallAsConstructor(iterable);
    }

    public int Count => (int)_value["size"];

    public JSIterable.Enumerator GetEnumerator() => new(_value);

    public bool Add(JSValue item)
    {
        int countBeforeAdd = Count;
        _value.CallMethod("add", item);
        return Count > countBeforeAdd;
    }

    public bool Remove(JSValue item) => (bool)_value.CallMethod("delete", item);

    public void Clear() => _value.CallMethod("clear");

    public bool Contains(JSValue item) => (bool)_value.CallMethod("has", item);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSSet a, JSSet b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSSet a, JSSet b) => !a._value.StrictEquals(b);

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
