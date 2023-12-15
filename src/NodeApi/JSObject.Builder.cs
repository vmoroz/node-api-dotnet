// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.JavaScript.NodeApi;

//TODO: (vmoroz) [CollectionBuilder(typeof(Builder), "Create")]
public ref partial struct JSObject
{
    // TODO: (vmoroz) Implement this
    public struct Builder : IEnumerable<Builder>
    {
        public static implicit operator JSObject(Builder builder) => builder.AsObject();

        public IEnumerator<Builder> GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public void Add(KeyValuePair<JSValueChecked, JSValueChecked> item) => throw new NotImplementedException();

        public JSObject AsObject() => throw new NotImplementedException();
    }
}
