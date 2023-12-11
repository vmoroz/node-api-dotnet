// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.JavaScript.NodeApi;

public partial struct JSArray
{
    public ref struct Enumerator
    {
        private readonly JSValue _array;
        private readonly int _count;
        private int _index;

        internal Enumerator(JSValue array)
        {
            _array = array;
            if (array.IsArray())
            {
                _count = array.GetArrayLength();
            }
            else
            {
                _count = 0;
            }
            _index = -1;
        }

        public bool MoveNext()
        {
            if (_index + 1 < _count)
            {
                _index++;
                return true;
            }

            return false;
        }

        public readonly JSValue Current => _array[_index];
    }
}
