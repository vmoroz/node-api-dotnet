// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.JavaScript.NodeApi;

public partial struct JSAsyncIterable
{
    public struct Enumerator : IAsyncEnumerator<JSReference>
    {
        private readonly JSReference _iterable;
        private readonly JSReference _iterator;
        private JSReference? _current;

        internal Enumerator(JSValue iterable)
        {
            _iterable = new JSReference(iterable);
            _iterator = new JSReference(iterable.CallMethod(JSSymbol.AsyncIterator));
            _current = default;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            var nextPromise = new JSReference(_iterator.GetValue().CallMethod("next"));
            JSReference nextResult = await ((JSPromise)nextPromise.GetValue()).AsTask();
            return MoveNextAsyncCore(nextResult);
        }

        private bool MoveNextAsyncCore(JSReference nextResult)
        {
            JSValue done = nextResult.GetValue()["done"];
            if (done.IsBoolean() && (bool)done)
            {
                _current = default;
                return false;
            }
            else
            {
                _current = new JSReference(nextResult.GetValue()["value"]);
                return true;
            }
        }

        public readonly JSReference Current
            => _current ?? throw new InvalidOperationException("Unexpected enumerator state");

        readonly ValueTask IAsyncDisposable.DisposeAsync() => default;
    }
}

