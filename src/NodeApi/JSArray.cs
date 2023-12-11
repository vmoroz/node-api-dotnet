// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.JavaScript.NodeApi;

public readonly ref partial struct JSArray
{
    private readonly JSValue _value;

    public static explicit operator JSArray(JSValue value) => new(value);
    public static implicit operator JSValue(JSArray arr) => arr._value;

    public static explicit operator JSArray(JSObject obj) => (JSArray)(JSValue)obj;
    public static implicit operator JSObject(JSArray arr) => (JSObject)arr._value;

    public static explicit operator JSArray(JSIterable obj) => (JSArray)(JSValue)obj;
    public static implicit operator JSIterable(JSArray arr) => (JSIterable)arr._value;

    private JSArray(JSValue value)
    {
        _value = value;
    }

    public JSArray() : this(JSValue.CreateArray())
    {
    }

    public JSArray(int length) : this(JSValue.CreateArray(length))
    {
    }

    public int Length => _value.GetArrayLength();

    public JSValue this[int index]
    {
        get => _value.GetElement(index);
        set => _value.SetElement(index, value);
    }

    public void Add(JSValue item) => _value["push"].Call(_value, item);

    public Enumerator GetEnumerator() => new(_value);

    public int IndexOf(JSValue item) => (int)_value.CallMethod("indexOf", item);

    public void Insert(int index, JSValue item) => _value.CallMethod("splice", index, 0, item);

    public void RemoveAt(int index) => _value.CallMethod("splice", index, 1);

    public void Clear() => _value.CallMethod("splice", 0, Length);

    public bool Contains(JSValue item) => IndexOf(item) >= 0;

    public bool Remove(JSValue item)
    {
        int index = IndexOf(item);
        if (index < 0) return false;
        RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSArray a, JSArray b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSArray a, JSArray b) => !a._value.StrictEquals(b);

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
       => throw new NotSupportedException(
           "Equals for JS values is not supported. Use == operator.");

    /// <inheritdoc/>
    public override int GetHashCode()
        => throw new NotSupportedException(
           "Hashing JS values is not supported. Use JSSet or JSMap instead.");
}
