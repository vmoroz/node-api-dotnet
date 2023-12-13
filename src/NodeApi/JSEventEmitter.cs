// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.JavaScript.NodeApi.Runtime.JSRuntime;

namespace Microsoft.JavaScript.NodeApi;

/// <summary>
/// Event emitter that can be backed by either a Node.js `EventEmitter` object or
/// a standalone (runtime-agnostic) listener tracker.
/// </summary>
public class JSEventEmitter : IDisposable
{
    private readonly JSReference? _nodeEmitter;
    private readonly Dictionary<string, JSReference>? _listeners;

    /// <summary>
    /// Creates a new instance of a standalone (runtime-agnostic) event emitter.
    /// </summary>
    public JSEventEmitter()
    {
        _listeners = new Dictionary<string, JSReference>();
    }

    /// <summary>
    /// Creates a new instance of an event emitter backed by a Node.js `EventEmitter` object.
    /// </summary>
    public JSEventEmitter(JSValue nodejsEventEmitter)
    {
        if (!nodejsEventEmitter.IsObject())
        {
            throw new ArgumentException("Event emitter must be an object.");
        }

        _nodeEmitter = new JSReference(nodejsEventEmitter);
    }

    public void AddListener(string eventName, JSValue listener)
    {
        if (_nodeEmitter != null)
        {
            _nodeEmitter.GetValue().CallMethod("addListener", eventName, listener);
            return;
        }

        if (!listener.IsFunction())
        {
            throw new ArgumentException("Listener must be a function.");
        }

        JSArray eventListeners;
        if (_listeners!.TryGetValue(eventName, out JSReference? eventListenersReference))
        {
            eventListeners = (JSArray)eventListenersReference.GetValue();
        }
        else
        {
            eventListeners = new JSArray();
            _listeners.Add(eventName, new JSReference(eventListeners));
        }

        eventListeners.Add(listener);
    }

    public void RemoveListener(string eventName, JSValue listener)
    {
        if (_nodeEmitter != null)
        {
            _nodeEmitter.GetValue().CallMethod("removeListener", eventName, listener);
            return;
        }

        if (_listeners!.TryGetValue(eventName, out JSReference? eventListenersReference))
        {
            JSArray eventListeners = (JSArray)eventListenersReference.GetValue();
            eventListeners.Remove(listener);
        }
    }

    public void Once(string eventName, JSCallbackFunc listener)
    {
        if (_nodeEmitter != null)
        {
            _nodeEmitter.GetValue().CallMethod(
                "once", eventName, JSValue.CreateFunction(eventName, listener));
            return;
        }

        JSReference? onceListenerRef = default;
        JSValue onceListener = JSValue.CreateFunction(eventName, (args) =>
        {
            listener(args);
            RemoveListener(eventName, onceListenerRef!.GetValue());
            return default;
        });
        onceListenerRef = new JSReference(onceListener);

        AddListener(eventName, onceListener);
    }

    public void Once(string eventName, JSValue listener)
    {
        if (_nodeEmitter != null)
        {
            _nodeEmitter.GetValue().CallMethod("once", eventName, listener);
            return;
        }

        JSReference listenerRef = new JSReference(listener);
        JSReference? onceListenerRef = default;
        JSValue onceListener = JSValue.CreateFunction(eventName, (args) =>
        {
            listenerRef.GetValue().Call(args.ThisArg, args.Arguments);
            RemoveListener(eventName, onceListenerRef!.GetValue());
            return default;
        });
        onceListenerRef = new JSReference(onceListener);

        AddListener(eventName, onceListener);
    }

    public void Emit(string eventName)
    {
        if (_nodeEmitter != null)
        {
            _nodeEmitter.GetValue().CallMethod("emit", eventName);
            return;
        }

        if (_listeners!.TryGetValue(eventName, out JSReference? eventListenersReference))
        {
            JSArray eventListeners = (JSArray)eventListenersReference.GetValue();
            foreach (JSValue listener in eventListeners)
            {
                listener.Call(thisArg: default);
            }
        }
    }

    public void Emit(string eventName, JSValue arg)
    {
        if (_nodeEmitter != null)
        {
            _nodeEmitter.GetValue().CallMethod("emit", eventName, arg);
            return;
        }

        if (_listeners!.TryGetValue(eventName, out JSReference? eventListenersReference))
        {
            JSArray eventListeners = (JSArray)eventListenersReference.GetValue();
            foreach (JSValue listener in eventListeners)
            {
                listener.Call(thisArg: default, arg);
            }
        }
    }

    public void Emit(string eventName, params JSValueChecked[] args)
    {
        switch (args.Length)
        {
            case 0:
                Emit(eventName);
                break;
            case 1:
                Emit(eventName, (JSValue)args[0]);
                break;
            default:
                Span<napi_value> argSpan = stackalloc napi_value[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    argSpan[i] = args[i].Handle;
                }
                Emit(eventName, new JSValueReadOnlySpan(JSValueScope.Current, argSpan));
                break;
        }
    }

    public void Emit(string eventName, JSValueReadOnlySpan args)
    {
        if (_nodeEmitter != null)
        {
            Span<napi_value> argValues = stackalloc napi_value[args.Length + 1];
            JSValueSpan argsSpan = new JSValueSpan(argValues);
            argsSpan[0] = eventName;
            args.CopyTo(argsSpan, 1);
            _nodeEmitter.GetValue().CallMethod("emit", argsSpan);
            return;
        }

        if (_listeners!.TryGetValue(eventName, out JSReference? eventListenersReference))
        {
            JSArray eventListeners = (JSArray)eventListenersReference.GetValue();
            foreach (JSValue listener in eventListeners)
            {
                listener.Call(thisArg: default, args);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_nodeEmitter != null)
        {
            _nodeEmitter.Dispose();
        }
        else
        {
            _listeners!.Values.ToList().ForEach(l => l.Dispose());
            _listeners.Clear();
        }
    }
}
