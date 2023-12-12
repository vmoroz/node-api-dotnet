// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.JavaScript.NodeApi;

public readonly ref struct JSValueKeyValuePair
{
    internal JSValueKeyValuePair(JSValue key, JSValue value)
    {
        Key = key;
        Value = value;
    }

    public JSValue Key { get; }

    public JSValue Value { get; }
}
