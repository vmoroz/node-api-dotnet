// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.JavaScript.NodeApi;

public ref partial struct JSObject
{
    public ref struct Enumerator
    {
        private readonly JSValue _value;
        private readonly JSValue _names;
        private readonly int _count;
        private int _index;
        private JSValueKeyValuePair _current;

        internal Enumerator(JSValue value)
        {
            _value = value;
            JSValueType valueType = value.TypeOf();
            if (valueType == JSValueType.Object || valueType == JSValueType.Function)
            {
                JSValue names = value.GetPropertyNames();
                _names = names;
                _count = names.GetArrayLength();
            }
            else
            {
                _names = JSValue.Undefined;
                _count = 0;
            }
            _index = -1;
            _current = default;
        }

        public bool MoveNext()
        {
            if (++_index < _count)
            {
                JSValue name = _names.GetElement(_index);
                _current = new JSValueKeyValuePair(name, _value.GetProperty(name));
                return true;
            }

            _current = default;
            return false;
        }

        public readonly JSValueKeyValuePair Current
            => ((uint)_index < (uint)_count) ? _current : throw new IndexOutOfRangeException();
    }
}
