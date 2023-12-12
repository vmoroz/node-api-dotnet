// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.JavaScript.NodeApi;

public ref partial struct JSMap
{
    public ref struct Enumerator
    {
        private readonly JSValue _iterable;
        private JSValue _iterator;
        private JSValueKeyValuePair _current;
        private bool _hasCurrent;

        internal Enumerator(JSValue iterable)
        {
            _iterable = iterable;
            _iterator = _iterable.CallMethod(JSSymbol.Iterator);
        }

        public bool MoveNext()
        {
            JSValue nextResult = _iterator.CallMethod("next");
            JSValue done = nextResult["done"];
            if (done.IsBoolean() && (bool)done)
            {
                _current = default;
                _hasCurrent = false;
                return false;
            }
            else
            {
                JSArray currentEntry = (JSArray)nextResult["value"];
                _current = new JSValueKeyValuePair(currentEntry[0], currentEntry[1]);
                _hasCurrent = true;
                return true;
            }
        }

        public readonly JSValueKeyValuePair Current
            => _hasCurrent ? _current : throw new InvalidOperationException("Unexpected enumerator state");
    }
}
