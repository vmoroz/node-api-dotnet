// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.JavaScript.NodeApi;

public ref partial struct JSMap
{
    public struct Enumerator :
           IEnumerator<KeyValuePair<JSValue.Checked, JSValue.Checked>>,
           System.Collections.IEnumerator
    {
        private readonly JSValue.Checked _iterable;
        private JSValue.Checked _iterator;
        private KeyValuePair<JSValue.Checked, JSValue.Checked>? _current;

        internal Enumerator(JSValue iterable)
        {
            _iterable = iterable;
            _iterator = iterable.CallMethod(JSSymbol.Iterator);
            _current = default;
        }

        public bool MoveNext()
        {
            JSValue nextResult = _iterator.ToValue().CallMethod("next");
            JSValue done = nextResult["done"];
            if (done.IsBoolean() && (bool)done)
            {
                _current = default;
                return false;
            }
            else
            {
                JSArray currentEntry = (JSArray)nextResult["value"];
                _current = new KeyValuePair<JSValue.Checked, JSValue.Checked>(
                    currentEntry[0], currentEntry[1]);
                return true;
            }
        }

        public readonly KeyValuePair<JSValue.Checked, JSValue.Checked> Current
            => _current ?? throw new InvalidOperationException("Unexpected enumerator state");

        readonly object? System.Collections.IEnumerator.Current => Current;

        void System.Collections.IEnumerator.Reset()
        {
            _iterator = _iterable.ToValue().CallMethod(JSSymbol.Iterator);
            _current = default;
        }

        readonly void IDisposable.Dispose()
        {
        }
    }
}

