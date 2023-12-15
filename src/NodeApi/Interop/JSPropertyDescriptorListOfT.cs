// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.JavaScript.NodeApi.Interop;

public abstract class JSPropertyDescriptorList<TDerived, TObject>
  where TDerived : class
  where TObject : class
{
    public delegate TObject? Unwrap(JSCallbackArgs args);

    private readonly Unwrap _unwrap;

    public IList<JSPropertyDescriptor> Properties { get; } = new List<JSPropertyDescriptor>();

    protected JSPropertyDescriptorList(Unwrap unwrap)
    {
        _unwrap = unwrap;
    }

    /// <summary>
    /// Adds a property with an initial value of undefined.
    /// </summary>
    public TDerived AddProperty(
        string name,
        JSPropertyAttributes attributes = JSPropertyAttributes.DefaultProperty)
    {
        Properties.Add(JSPropertyDescriptor.ForValue(name, JSValue.Undefined, attributes));
        return (TDerived)(object)this;
    }

    /// <summary>
    /// Adds a property with a specified initial value.
    /// </summary>
    public TDerived AddProperty(
      string name,
      JSValue value,
      JSPropertyAttributes attributes = JSPropertyAttributes.DefaultProperty)
    {
        Properties.Add(JSPropertyDescriptor.ForValue(name, value, attributes));
        return (TDerived)(object)this;
    }

    /// <summary>
    /// Adds a property with getter and/or setter callbacks.
    /// </summary>
    private TDerived AddProperty(
      string name,
      JSCallbackFunc? getter,
      JSCallbackFunc? setter,
      JSPropertyAttributes attributes = JSPropertyAttributes.DefaultProperty,
      object? data = null)
    {
        Properties.Add(JSPropertyDescriptor.Accessor(name, getter, setter, attributes, data));
        return (TDerived)(object)this;
    }

    /// <summary>
    /// Adds a property with getter and/or setter callbacks.
    /// </summary>
    public TDerived AddProperty(
      string name,
      JSCallbackFunc0? getter,
      JSCallbackAction1? setter,
      JSPropertyAttributes attributes = JSPropertyAttributes.DefaultProperty)
    {
        return AddProperty(
          name,
          getter == null ? null : args => getter(),
          setter == null ? null : args =>
          {
              setter(args[0]);
              return JSValue.Undefined;
          },
          attributes);
    }

    /// <summary>
    /// Adds a property with getter and/or setter callbacks obtained from the wrapped object.
    /// </summary>
    public TDerived AddProperty(
      string name,
      Func<TObject, JSValue.Checked>? getter,
      Action<TObject, JSValue.Checked>? setter,
      JSPropertyAttributes attributes = JSPropertyAttributes.DefaultProperty)
    {
        return AddProperty(
          name,
          getter == null ? null : args =>
          {
              return (_unwrap(args) is TObject obj) ? getter(obj).Value : JSValue.Undefined;
          },
          setter == null ? null : args =>
          {
              if (_unwrap(args) is TObject obj)
              {
                  setter(obj, args[0]);
              }
              return JSValue.Undefined;
          },
          attributes);
    }

    /// <summary>
    /// Adds a method with a void no-args callback.
    /// </summary>
    public TDerived AddMethod(
      string name,
      JSCallbackAction0 callback,
      JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod)
    {
        return AddMethod(
          name,
          args =>
          {
              callback.Invoke();
              return JSValue.Undefined;
          },
          attributes);
    }

    /// <summary>
    /// Adds a method with a void callback.
    /// </summary>
    public TDerived AddMethod(
      string name,
      JSCallbackAction callback,
      JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod,
      object? data = null)
    {
        return AddMethod(
          name,
          args =>
          {
              callback.Invoke(args);
              return JSValue.Undefined;
          },
          attributes,
          data);
    }

    /// <summary>
    /// Adds a method with a callback.
    /// </summary>
    public TDerived AddMethod(
      string name,
      JSCallbackFunc callback,
      JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod,
      object? data = null)
    {
        Properties.Add(JSPropertyDescriptor.Function(name, callback, attributes, data));
        return (TDerived)(object)this;
    }

    /// <summary>
    /// Adds a method with a void no-args callback obtained from the wrapped object.
    /// </summary>
    public TDerived AddMethod(
      string name,
      Func<TObject, Action> getCallback,
      JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod)
    {
        return AddMethod(
          name,
          args =>
          {
              if (_unwrap(args) is TObject obj)
              {
                  getCallback(obj).Invoke();
              }
              return JSValue.Undefined;
          },
          attributes);
    }

    /// <summary>
    /// Adds a method with a void callback obtained from the wrapped object.
    /// </summary>
    public TDerived AddMethod(
      string name,
      Func<TObject, JSCallbackAction> getCallback,
      JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod,
      object? data = null)
    {
        return AddMethod(
          name,
          args =>
          {
              if (_unwrap(args) is TObject obj)
              {
                  getCallback(obj).Invoke(args);
              }
              return JSValue.Undefined;
          },
          attributes,
          data);
    }

    /// <summary>
    /// Adds a method with a callback obtained from the wrapped object.
    /// </summary>
    public TDerived AddMethod(
      string name,
      Func<TObject, JSCallbackFunc> getCallback,
      JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod,
      object? data = null)
    {
        return AddMethod(
          name,
          args => (_unwrap(args) is TObject obj) ?
            getCallback(obj).Invoke(args) : JSValue.Undefined,
          attributes,
          data);
    }
}
