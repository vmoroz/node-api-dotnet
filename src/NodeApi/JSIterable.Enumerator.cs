// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.JavaScript.NodeApi;

public ref partial struct JSIterable
{
    public ref struct Enumerator
    {
        private readonly JSValue _iterable;
        private JSValue _iterator;
        private JSValue _current = default;
        private bool _hasCurrent = false;

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
                _current = nextResult["value"];
                _hasCurrent = true;
                return true;
            }
        }

        public readonly JSValue Current
            => _hasCurrent ? _current : throw new InvalidOperationException("Unexpected enumerator state");
    }
}

