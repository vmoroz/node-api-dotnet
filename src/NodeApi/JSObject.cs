// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.JavaScript.NodeApi;

public readonly ref partial struct JSObject
{
    private readonly JSValue _value;

    public static explicit operator JSObject(JSValue value) => new(value);
    public static implicit operator JSValue(JSObject obj) => obj._value;

    private JSObject(JSValue value)
    {
        _value = value;
    }

    public JSObject() : this(JSValue.CreateObject())
    {
    }

    public void DefineProperties(params JSPropertyDescriptor[] descriptors)
    {
        _value.DefineProperties(descriptors);
    }

    public void DefineProperties(IReadOnlyCollection<JSPropertyDescriptor> descriptors)
    {
        _value.DefineProperties(descriptors);
    }

    public JSObject Wrap(object target)
    {
        JSNativeApi.Wrap(_value, target);
        return this;
    }

    public bool TryUnwrap<T>(out T? target) where T : class
    {
        if (!JSNativeApi.TryUnwrap(_value, out object? unwrapped))
        {
            target = null;
            return false;
        }

        target = unwrapped as T;
        return true;
    }

    public T Unwrap<T>() where T : class
    {
        return (T)JSNativeApi.Unwrap(_value, typeof(T).Name);
    }

    public JSValue this[JSValue name]
    {
        get => _value.GetProperty(name);
        set => _value.SetProperty(name, value);
    }

    public JSValue this[string name]
    {
        get => _value.GetProperty(name);
        set => _value.SetProperty(name, value);
    }

    public JSValue CallMethod(JSValue methodName)
        => _value.GetProperty(methodName).Call(_value);

    public JSValue CallMethod(JSValue methodName, JSValue arg0)
        => _value.GetProperty(methodName).Call(_value, arg0);

    public JSValue CallMethod(JSValue methodName, JSValue arg0, JSValue arg1)
        => _value.GetProperty(methodName).Call(_value, arg0, arg1);

    public JSValue CallMethod(JSValue methodName, JSValue arg0, JSValue arg1, JSValue arg2)
        => _value.GetProperty(methodName).Call(_value, arg0, arg1, arg2);

    public JSValue CallMethod(JSValue methodName, JSValueReadOnlySpan args)
        => _value.GetProperty(methodName).Call(_value, args);

    public void Add(JSValue key, JSValue value) => _value.SetProperty(key, value);

    public bool ContainsKey(JSValue key) => _value.HasProperty(key);

    public bool Remove(JSValue key) => _value.DeleteProperty(key);

    public bool TryGetValue(JSValue key, [MaybeNullWhen(false)] out JSValue value)
    {
        value = _value.GetProperty(key);
        return !value.IsUndefined();
    }

    public Enumerator GetEnumerator() => new(_value);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSObject a, JSObject b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSObject a, JSObject b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is JSReference other && Equals(other.GetValue());

    public override int GetHashCode()
        => throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
}
