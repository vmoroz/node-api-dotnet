// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.JavaScript.NodeApi.Interop;

namespace Microsoft.JavaScript.NodeApi;

public readonly ref struct JSPropertyDescriptor
{
    /// <summary>
    /// Saves the module context under which the callback was defined, so that multiple .NET
    /// modules in the same process can register callbacks for module-level functions.
    /// </summary>
    internal JSModuleContext? ModuleContext { get; init; }

    // Either Name or NameValue should be non-null.
    // NameValue supports non-string property names like symbols.
    public string? Name { get; }
    public JSValue NameValue { get; }

    public JSCallbackFunc? Method { get; }
    public JSCallbackFunc? Getter { get; }
    public JSCallbackFunc? Setter { get; }
    public JSValue Value { get; }
    public JSPropertyAttributes Attributes { get; }
    public object? Data { get; }

    public JSPropertyDescriptor(
        string name,
        JSCallbackFunc? method = null,
        JSCallbackFunc? getter = null,
        JSCallbackFunc? setter = null,
        JSValue value = default,
        JSPropertyAttributes attributes = JSPropertyAttributes.Default,
        object? data = null)
    {
        ModuleContext = JSValueScope.Current.ModuleContext;

        Name = name;
        Method = method;
        Getter = getter;
        Setter = setter;
        Value = value;
        Attributes = attributes;
        Data = data;
    }

    public JSPropertyDescriptor(
        JSValue name,
        JSCallbackFunc? method = null,
        JSCallbackFunc? getter = null,
        JSCallbackFunc? setter = null,
        JSValue value = default,
        JSPropertyAttributes attributes = JSPropertyAttributes.Default,
        object? data = null)
    {
        ModuleContext = JSValueScope.Current.ModuleContext;

        NameValue = name;
        Method = method;
        Getter = getter;
        Setter = setter;
        Value = value;
        Attributes = attributes;
        Data = data;
    }

    public static JSPropertyDescriptor Accessor(
        string name,
        JSCallbackFunc? getter = null,
        JSCallbackFunc? setter = null,
        JSPropertyAttributes attributes = JSPropertyAttributes.Default,
        object? data = null)
    {
        if (getter == null && setter == null)
        {
            throw new ArgumentException($"Either `{nameof(getter)}` or `{nameof(setter)}` or both must be not null");
        }

        return new JSPropertyDescriptor(name, null, getter, setter, JSValue.Undefined, attributes, data);
    }

    public static JSPropertyDescriptor ForValue(
        string name,
        JSValue value,
        JSPropertyAttributes attributes = JSPropertyAttributes.Default,
        object? data = null)
    {
        return new JSPropertyDescriptor(name, null, null, null, value, attributes, data);
    }

    public static JSPropertyDescriptor Function(
        string name,
        JSCallbackFunc method,
        JSPropertyAttributes attributes = JSPropertyAttributes.Default,
        object? data = null)
    {
        return new JSPropertyDescriptor(name, method, null, null, JSValue.Undefined, attributes, data);
    }
}
