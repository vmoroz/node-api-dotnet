// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.JavaScript.NodeApi;

public readonly ref struct JSInt8Array
{
    private readonly JSValue _value;

    public static explicit operator JSInt8Array(JSValue value) => new(value);
    public static implicit operator JSValue(JSInt8Array arr) => arr._value;

    private static int ElementSize => sizeof(sbyte);

    private static JSTypedArrayType ArrayType => JSTypedArrayType.Int8;

    private JSInt8Array(JSValue value) => _value = value;

    /// <summary>
    /// Creates a new typed array of specified length, with newly allocated memory.
    /// </summary>
    public JSInt8Array(int length)
    {
        JSValue arrayBuffer = JSValue.CreateArrayBuffer(length * ElementSize);
        _value = JSValue.CreateTypedArray(ArrayType, length, arrayBuffer, 0);
    }

    /// <summary>
    /// Creates a typed-array over memory, without copying.
    /// </summary>
    public unsafe JSInt8Array(Memory<sbyte> data)
    {
        JSValue value = JSTypedArrayMemoryManager<sbyte>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // The Memory was NOT created from a JS TypedArray. Most likely it was allocated
            // directly or via a .NET array or string.

            JSValue arrayBuffer = data.Length > 0 ?
                JSValue.CreateExternalArrayBuffer(data) : JSValue.CreateArrayBuffer(0);
            _value = JSValue.CreateTypedArray(ArrayType, data.Length, arrayBuffer, 0);
        }
    }

    /// <summary>
    /// Creates a typed-array over read-memory, without copying. Only valid for memory
    /// which was previously marshaled from a JS typed-array to .NET.
    /// </summary>
    /// <exception cref="NotSupportedException">The memory is external to JS.</exception>
    public unsafe JSInt8Array(ReadOnlyMemory<sbyte> data)
    {
        JSValue value = JSTypedArrayMemoryManager<sbyte>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // Consider copying the memory?
            throw new NotSupportedException(
                "Read-only memory cannot be transferred from .NET to JS.");
        }
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSInt8Array(sbyte[] data) : this(data.AsMemory())
    {
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSInt8Array(sbyte[] data, int start, int length)
        : this(data.AsMemory().Slice(start, length))
    {
    }

    public int Length => _value.GetTypedArrayLength(out _);

    public sbyte this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    /// <summary>
    /// Gets the typed-array values as a span, without copying.
    /// </summary>
    public Span<sbyte> Span => _value.GetTypedArrayData<sbyte>();

    /// <summary>
    /// Gets the typed-array values as memory, without copying.
    /// </summary>
    public Memory<sbyte> Memory => new JSTypedArrayMemoryManager<sbyte>(this, Span).Memory;

    /// <summary>
    /// Copies the typed-array data into a new array and returns the array.
    /// </summary>
    public sbyte[] ToArray() => Span.ToArray();

    /// <summary>
    /// Copies the typed-array data into an array.
    /// </summary>
    public void CopyTo(sbyte[] array, int arrayIndex)
    {
        Span.CopyTo(new Span<sbyte>(array, arrayIndex, array.Length - arrayIndex));
    }

    public Span<sbyte>.Enumerator GetEnumerator() => Span.GetEnumerator();

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSInt8Array a, JSInt8Array b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSInt8Array a, JSInt8Array b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}

public readonly ref struct JSUInt8Array
{
    private readonly JSValue _value;

    public static explicit operator JSUInt8Array(JSValue value) => new(value);
    public static implicit operator JSValue(JSUInt8Array arr) => arr._value;

    private static int ElementSize => sizeof(byte);

    private static JSTypedArrayType ArrayType => JSTypedArrayType.UInt8;

    private JSUInt8Array(JSValue value) => _value = value;

    /// <summary>
    /// Creates a new typed array of specified length, with newly allocated memory.
    /// </summary>
    public JSUInt8Array(int length)
    {
        JSValue arrayBuffer = JSValue.CreateArrayBuffer(length * ElementSize);
        _value = JSValue.CreateTypedArray(ArrayType, length, arrayBuffer, 0);
    }

    /// <summary>
    /// Creates a typed-array over memory, without copying.
    /// </summary>
    public unsafe JSUInt8Array(Memory<byte> data)
    {
        JSValue value = JSTypedArrayMemoryManager<byte>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // The Memory was NOT created from a JS TypedArray. Most likely it was allocated
            // directly or via a .NET array or string.

            JSValue arrayBuffer = data.Length > 0 ?
                JSValue.CreateExternalArrayBuffer(data) : JSValue.CreateArrayBuffer(0);
            _value = JSValue.CreateTypedArray(ArrayType, data.Length, arrayBuffer, 0);
        }
    }

    /// <summary>
    /// Creates a typed-array over read-memory, without copying. Only valid for memory
    /// which was previously marshaled from a JS typed-array to .NET.
    /// </summary>
    /// <exception cref="NotSupportedException">The memory is external to JS.</exception>
    public unsafe JSUInt8Array(ReadOnlyMemory<byte> data)
    {
        JSValue value = JSTypedArrayMemoryManager<byte>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // Consider copying the memory?
            throw new NotSupportedException(
                "Read-only memory cannot be transferred from .NET to JS.");
        }
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSUInt8Array(byte[] data) : this(data.AsMemory())
    {
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSUInt8Array(byte[] data, int start, int length)
        : this(data.AsMemory().Slice(start, length))
    {
    }

    public int Length => _value.GetTypedArrayLength(out _);

    public byte this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    /// <summary>
    /// Gets the typed-array values as a span, without copying.
    /// </summary>
    public Span<byte> Span => _value.GetTypedArrayData<byte>();

    /// <summary>
    /// Gets the typed-array values as memory, without copying.
    /// </summary>
    public Memory<byte> Memory => new JSTypedArrayMemoryManager<byte>(this, Span).Memory;

    /// <summary>
    /// Copies the typed-array data into a new array and returns the array.
    /// </summary>
    public byte[] ToArray() => Span.ToArray();

    /// <summary>
    /// Copies the typed-array data into an array.
    /// </summary>
    public void CopyTo(byte[] array, int arrayIndex)
    {
        Span.CopyTo(new Span<byte>(array, arrayIndex, array.Length - arrayIndex));
    }

    public Span<byte>.Enumerator GetEnumerator() => Span.GetEnumerator();

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSUInt8Array a, JSUInt8Array b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSUInt8Array a, JSUInt8Array b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}

public readonly ref struct JSUInt8ClampedArray
{
    private readonly JSValue _value;

    public static explicit operator JSUInt8ClampedArray(JSValue value) => new(value);
    public static implicit operator JSValue(JSUInt8ClampedArray arr) => arr._value;

    private static int ElementSize => sizeof(byte);

    private static JSTypedArrayType ArrayType => JSTypedArrayType.UInt8Clamped;

    private JSUInt8ClampedArray(JSValue value) => _value = value;

    /// <summary>
    /// Creates a new typed array of specified length, with newly allocated memory.
    /// </summary>
    public JSUInt8ClampedArray(int length)
    {
        JSValue arrayBuffer = JSValue.CreateArrayBuffer(length * ElementSize);
        _value = JSValue.CreateTypedArray(ArrayType, length, arrayBuffer, 0);
    }

    /// <summary>
    /// Creates a typed-array over memory, without copying.
    /// </summary>
    public unsafe JSUInt8ClampedArray(Memory<byte> data)
    {
        JSValue value = JSTypedArrayMemoryManager<byte>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // The Memory was NOT created from a JS TypedArray. Most likely it was allocated
            // directly or via a .NET array or string.

            JSValue arrayBuffer = data.Length > 0 ?
                JSValue.CreateExternalArrayBuffer(data) : JSValue.CreateArrayBuffer(0);
            _value = JSValue.CreateTypedArray(ArrayType, data.Length, arrayBuffer, 0);
        }
    }

    /// <summary>
    /// Creates a typed-array over read-memory, without copying. Only valid for memory
    /// which was previously marshaled from a JS typed-array to .NET.
    /// </summary>
    /// <exception cref="NotSupportedException">The memory is external to JS.</exception>
    public unsafe JSUInt8ClampedArray(ReadOnlyMemory<byte> data)
    {
        JSValue value = JSTypedArrayMemoryManager<byte>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // Consider copying the memory?
            throw new NotSupportedException(
                "Read-only memory cannot be transferred from .NET to JS.");
        }
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSUInt8ClampedArray(byte[] data) : this(data.AsMemory())
    {
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSUInt8ClampedArray(byte[] data, int start, int length)
        : this(data.AsMemory().Slice(start, length))
    {
    }

    public int Length => _value.GetTypedArrayLength(out _);

    public byte this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    /// <summary>
    /// Gets the typed-array values as a span, without copying.
    /// </summary>
    public Span<byte> Span => _value.GetTypedArrayData<byte>();

    /// <summary>
    /// Gets the typed-array values as memory, without copying.
    /// </summary>
    public Memory<byte> Memory => new JSTypedArrayMemoryManager<byte>(this, Span).Memory;

    /// <summary>
    /// Copies the typed-array data into a new array and returns the array.
    /// </summary>
    public byte[] ToArray() => Span.ToArray();

    /// <summary>
    /// Copies the typed-array data into an array.
    /// </summary>
    public void CopyTo(byte[] array, int arrayIndex)
    {
        Span.CopyTo(new Span<byte>(array, arrayIndex, array.Length - arrayIndex));
    }

    public Span<byte>.Enumerator GetEnumerator() => Span.GetEnumerator();

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSUInt8ClampedArray a, JSUInt8ClampedArray b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSUInt8ClampedArray a, JSUInt8ClampedArray b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}

public readonly ref struct JSInt16Array
{
    private readonly JSValue _value;

    public static explicit operator JSInt16Array(JSValue value) => new(value);
    public static implicit operator JSValue(JSInt16Array arr) => arr._value;

    private static int ElementSize => sizeof(short);

    private static JSTypedArrayType ArrayType => JSTypedArrayType.Int16;

    private JSInt16Array(JSValue value) => _value = value;

    /// <summary>
    /// Creates a new typed array of specified length, with newly allocated memory.
    /// </summary>
    public JSInt16Array(int length)
    {
        JSValue arrayBuffer = JSValue.CreateArrayBuffer(length * ElementSize);
        _value = JSValue.CreateTypedArray(ArrayType, length, arrayBuffer, 0);
    }

    /// <summary>
    /// Creates a typed-array over memory, without copying.
    /// </summary>
    public unsafe JSInt16Array(Memory<short> data)
    {
        JSValue value = JSTypedArrayMemoryManager<short>.GetJSValueForMemory(data); 
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // The Memory was NOT created from a JS TypedArray. Most likely it was allocated
            // directly or via a .NET array or string.

            JSValue arrayBuffer = data.Length > 0 ?
                JSValue.CreateExternalArrayBuffer(data) : JSValue.CreateArrayBuffer(0);
            _value = JSValue.CreateTypedArray(ArrayType, data.Length, arrayBuffer, 0);
        }
    }

    /// <summary>
    /// Creates a typed-array over read-memory, without copying. Only valid for memory
    /// which was previously marshaled from a JS typed-array to .NET.
    /// </summary>
    /// <exception cref="NotSupportedException">The memory is external to JS.</exception>
    public unsafe JSInt16Array(ReadOnlyMemory<short> data)
    {
        JSValue value = JSTypedArrayMemoryManager<short>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // Consider copying the memory?
            throw new NotSupportedException(
                "Read-only memory cannot be transferred from .NET to JS.");
        }
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSInt16Array(short[] data) : this(data.AsMemory())
    {
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSInt16Array(short[] data, int start, int length)
        : this(data.AsMemory().Slice(start, length))
    {
    }

    public int Length => _value.GetTypedArrayLength(out _);

    public short this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    /// <summary>
    /// Gets the typed-array values as a span, without copying.
    /// </summary>
    public Span<short> Span => _value.GetTypedArrayData<short>();

    /// <summary>
    /// Gets the typed-array values as memory, without copying.
    /// </summary>
    public Memory<short> Memory => new JSTypedArrayMemoryManager<short>(this, Span).Memory;

    /// <summary>
    /// Copies the typed-array data into a new array and returns the array.
    /// </summary>
    public short[] ToArray() => Span.ToArray();

    /// <summary>
    /// Copies the typed-array data into an array.
    /// </summary>
    public void CopyTo(short[] array, int arrayIndex)
    {
        Span.CopyTo(new Span<short>(array, arrayIndex, array.Length - arrayIndex));
    }

    public Span<short>.Enumerator GetEnumerator() => Span.GetEnumerator();

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSInt16Array a, JSInt16Array b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSInt16Array a, JSInt16Array b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}

public readonly ref struct JSUInt16Array
{
    private readonly JSValue _value;

    public static explicit operator JSUInt16Array(JSValue value) => new(value);
    public static implicit operator JSValue(JSUInt16Array arr) => arr._value;

    private static int ElementSize => sizeof(ushort);

    private static JSTypedArrayType ArrayType => JSTypedArrayType.UInt16;

    private JSUInt16Array(JSValue value) => _value = value;

    /// <summary>
    /// Creates a new typed array of specified length, with newly allocated memory.
    /// </summary>
    public JSUInt16Array(int length)
    {
        JSValue arrayBuffer = JSValue.CreateArrayBuffer(length * ElementSize);
        _value = JSValue.CreateTypedArray(ArrayType, length, arrayBuffer, 0);
    }

    /// <summary>
    /// Creates a typed-array over memory, without copying.
    /// </summary>
    public unsafe JSUInt16Array(Memory<ushort> data)
    {
        JSValue value = JSTypedArrayMemoryManager<ushort>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // The Memory was NOT created from a JS TypedArray. Most likely it was allocated
            // directly or via a .NET array or string.

            JSValue arrayBuffer = data.Length > 0 ?
                JSValue.CreateExternalArrayBuffer(data) : JSValue.CreateArrayBuffer(0);
            _value = JSValue.CreateTypedArray(ArrayType, data.Length, arrayBuffer, 0);
        }
    }

    /// <summary>
    /// Creates a typed-array over read-memory, without copying. Only valid for memory
    /// which was previously marshaled from a JS typed-array to .NET.
    /// </summary>
    /// <exception cref="NotSupportedException">The memory is external to JS.</exception>
    public unsafe JSUInt16Array(ReadOnlyMemory<ushort> data)
    {
        JSValue value = JSTypedArrayMemoryManager<ushort>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // Consider copying the memory?
            throw new NotSupportedException(
                "Read-only memory cannot be transferred from .NET to JS.");
        }
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSUInt16Array(ushort[] data) : this(data.AsMemory())
    {
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSUInt16Array(ushort[] data, int start, int length)
        : this(data.AsMemory().Slice(start, length))
    {
    }

    public int Length => _value.GetTypedArrayLength(out _);

    public ushort this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    /// <summary>
    /// Gets the typed-array values as a span, without copying.
    /// </summary>
    public Span<ushort> Span => _value.GetTypedArrayData<ushort>();

    /// <summary>
    /// Gets the typed-array values as memory, without copying.
    /// </summary>
    public Memory<ushort> Memory => new JSTypedArrayMemoryManager<ushort>(this, Span).Memory;

    /// <summary>
    /// Copies the typed-array data into a new array and returns the array.
    /// </summary>
    public ushort[] ToArray() => Span.ToArray();

    /// <summary>
    /// Copies the typed-array data into an array.
    /// </summary>
    public void CopyTo(ushort[] array, int arrayIndex)
    {
        Span.CopyTo(new Span<ushort>(array, arrayIndex, array.Length - arrayIndex));
    }

    public Span<ushort>.Enumerator GetEnumerator() => Span.GetEnumerator();

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSUInt16Array a, JSUInt16Array b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSUInt16Array a, JSUInt16Array b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}

public readonly ref struct JSInt32Array
{
    private readonly JSValue _value;

    public static explicit operator JSInt32Array(JSValue value) => new(value);
    public static implicit operator JSValue(JSInt32Array arr) => arr._value;

    private static int ElementSize => sizeof(int);

    private static JSTypedArrayType ArrayType => JSTypedArrayType.Int32;

    private JSInt32Array(JSValue value) => _value = value;

    /// <summary>
    /// Creates a new typed array of specified length, with newly allocated memory.
    /// </summary>
    public JSInt32Array(int length)
    {
        JSValue arrayBuffer = JSValue.CreateArrayBuffer(length * ElementSize);
        _value = JSValue.CreateTypedArray(ArrayType, length, arrayBuffer, 0);
    }

    /// <summary>
    /// Creates a typed-array over memory, without copying.
    /// </summary>
    public unsafe JSInt32Array(Memory<int> data)
    {
        JSValue value = JSTypedArrayMemoryManager<int>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // The Memory was NOT created from a JS TypedArray. Most likely it was allocated
            // directly or via a .NET array or string.

            JSValue arrayBuffer = data.Length > 0 ?
                JSValue.CreateExternalArrayBuffer(data) : JSValue.CreateArrayBuffer(0);
            _value = JSValue.CreateTypedArray(ArrayType, data.Length, arrayBuffer, 0);
        }
    }

    /// <summary>
    /// Creates a typed-array over read-memory, without copying. Only valid for memory
    /// which was previously marshaled from a JS typed-array to .NET.
    /// </summary>
    /// <exception cref="NotSupportedException">The memory is external to JS.</exception>
    public unsafe JSInt32Array(ReadOnlyMemory<int> data)
    {
        JSValue value = JSTypedArrayMemoryManager<int>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // Consider copying the memory?
            throw new NotSupportedException(
                "Read-only memory cannot be transferred from .NET to JS.");
        }
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSInt32Array(int[] data) : this(data.AsMemory())
    {
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSInt32Array(int[] data, int start, int length)
        : this(data.AsMemory().Slice(start, length))
    {
    }

    public int Length => _value.GetTypedArrayLength(out _);

    public int this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    /// <summary>
    /// Gets the typed-array values as a span, without copying.
    /// </summary>
    public Span<int> Span => _value.GetTypedArrayData<int>();

    /// <summary>
    /// Gets the typed-array values as memory, without copying.
    /// </summary>
    public Memory<int> Memory => new JSTypedArrayMemoryManager<int>(this, Span).Memory;

    /// <summary>
    /// Copies the typed-array data into a new array and returns the array.
    /// </summary>
    public int[] ToArray() => Span.ToArray();

    /// <summary>
    /// Copies the typed-array data into an array.
    /// </summary>
    public void CopyTo(int[] array, int arrayIndex)
    {
        Span.CopyTo(new Span<int>(array, arrayIndex, array.Length - arrayIndex));
    }

    public Span<int>.Enumerator GetEnumerator() => Span.GetEnumerator();

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSInt32Array a, JSInt32Array b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSInt32Array a, JSInt32Array b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}

public readonly ref struct JSUInt32Array
{
    private readonly JSValue _value;

    public static explicit operator JSUInt32Array(JSValue value) => new(value);
    public static implicit operator JSValue(JSUInt32Array arr) => arr._value;

    private static int ElementSize => sizeof(uint);

    private static JSTypedArrayType ArrayType => JSTypedArrayType.UInt32;

    private JSUInt32Array(JSValue value) => _value = value;

    /// <summary>
    /// Creates a new typed array of specified length, with newly allocated memory.
    /// </summary>
    public JSUInt32Array(int length)
    {
        JSValue arrayBuffer = JSValue.CreateArrayBuffer(length * ElementSize);
        _value = JSValue.CreateTypedArray(ArrayType, length, arrayBuffer, 0);
    }

    /// <summary>
    /// Creates a typed-array over memory, without copying.
    /// </summary>
    public unsafe JSUInt32Array(Memory<uint> data)
    {
        JSValue value = JSTypedArrayMemoryManager<uint>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // The Memory was NOT created from a JS TypedArray. Most likely it was allocated
            // directly or via a .NET array or string.

            JSValue arrayBuffer = data.Length > 0 ?
                JSValue.CreateExternalArrayBuffer(data) : JSValue.CreateArrayBuffer(0);
            _value = JSValue.CreateTypedArray(ArrayType, data.Length, arrayBuffer, 0);
        }
    }

    /// <summary>
    /// Creates a typed-array over read-memory, without copying. Only valid for memory
    /// which was previously marshaled from a JS typed-array to .NET.
    /// </summary>
    /// <exception cref="NotSupportedException">The memory is external to JS.</exception>
    public unsafe JSUInt32Array(ReadOnlyMemory<uint> data)
    {
        JSValue value = JSTypedArrayMemoryManager<uint>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // Consider copying the memory?
            throw new NotSupportedException(
                "Read-only memory cannot be transferred from .NET to JS.");
        }
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSUInt32Array(uint[] data) : this(data.AsMemory())
    {
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSUInt32Array(uint[] data, int start, int length)
        : this(data.AsMemory().Slice(start, length))
    {
    }

    public int Length => _value.GetTypedArrayLength(out _);

    public uint this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    /// <summary>
    /// Gets the typed-array values as a span, without copying.
    /// </summary>
    public Span<uint> Span => _value.GetTypedArrayData<uint>();

    /// <summary>
    /// Gets the typed-array values as memory, without copying.
    /// </summary>
    public Memory<uint> Memory => new JSTypedArrayMemoryManager<uint>(this, Span).Memory;

    /// <summary>
    /// Copies the typed-array data into a new array and returns the array.
    /// </summary>
    public uint[] ToArray() => Span.ToArray();

    /// <summary>
    /// Copies the typed-array data into an array.
    /// </summary>
    public void CopyTo(uint[] array, int arrayIndex)
    {
        Span.CopyTo(new Span<uint>(array, arrayIndex, array.Length - arrayIndex));
    }

    public Span<uint>.Enumerator GetEnumerator() => Span.GetEnumerator();

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSUInt32Array a, JSUInt32Array b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSUInt32Array a, JSUInt32Array b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}

public readonly ref struct JSBigInt64Array
{
    private readonly JSValue _value;

    public static explicit operator JSBigInt64Array(JSValue value) => new(value);
    public static implicit operator JSValue(JSBigInt64Array arr) => arr._value;

    private static int ElementSize => sizeof(long);

    private static JSTypedArrayType ArrayType => JSTypedArrayType.BigInt64;

    private JSBigInt64Array(JSValue value) => _value = value;

    /// <summary>
    /// Creates a new typed array of specified length, with newly allocated memory.
    /// </summary>
    public JSBigInt64Array(int length)
    {
        JSValue arrayBuffer = JSValue.CreateArrayBuffer(length * ElementSize);
        _value = JSValue.CreateTypedArray(ArrayType, length, arrayBuffer, 0);
    }

    /// <summary>
    /// Creates a typed-array over memory, without copying.
    /// </summary>
    public unsafe JSBigInt64Array(Memory<long> data)
    {
        JSValue value = JSTypedArrayMemoryManager<long>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // The Memory was NOT created from a JS TypedArray. Most likely it was allocated
            // directly or via a .NET array or string.

            JSValue arrayBuffer = data.Length > 0 ?
                JSValue.CreateExternalArrayBuffer(data) : JSValue.CreateArrayBuffer(0);
            _value = JSValue.CreateTypedArray(ArrayType, data.Length, arrayBuffer, 0);
        }
    }

    /// <summary>
    /// Creates a typed-array over read-memory, without copying. Only valid for memory
    /// which was previously marshaled from a JS typed-array to .NET.
    /// </summary>
    /// <exception cref="NotSupportedException">The memory is external to JS.</exception>
    public unsafe JSBigInt64Array(ReadOnlyMemory<long> data)
    {
        JSValue value = JSTypedArrayMemoryManager<long>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // Consider copying the memory?
            throw new NotSupportedException(
                "Read-only memory cannot be transferred from .NET to JS.");
        }
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSBigInt64Array(long[] data) : this(data.AsMemory())
    {
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSBigInt64Array(long[] data, int start, int length)
        : this(data.AsMemory().Slice(start, length))
    {
    }

    public int Length => _value.GetTypedArrayLength(out _);

    public long this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    /// <summary>
    /// Gets the typed-array values as a span, without copying.
    /// </summary>
    public Span<long> Span => _value.GetTypedArrayData<long>();

    /// <summary>
    /// Gets the typed-array values as memory, without copying.
    /// </summary>
    public Memory<long> Memory => new JSTypedArrayMemoryManager<long>(this, Span).Memory;

    /// <summary>
    /// Copies the typed-array data into a new array and returns the array.
    /// </summary>
    public long[] ToArray() => Span.ToArray();

    /// <summary>
    /// Copies the typed-array data into an array.
    /// </summary>
    public void CopyTo(long[] array, int arrayIndex)
    {
        Span.CopyTo(new Span<long>(array, arrayIndex, array.Length - arrayIndex));
    }

    public Span<long>.Enumerator GetEnumerator() => Span.GetEnumerator();

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSBigInt64Array a, JSBigInt64Array b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSBigInt64Array a, JSBigInt64Array b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}

public readonly ref struct JSBigUInt64Array
{
    private readonly JSValue _value;

    public static explicit operator JSBigUInt64Array(JSValue value) => new(value);
    public static implicit operator JSValue(JSBigUInt64Array arr) => arr._value;

    private static int ElementSize => sizeof(ulong);

    private static JSTypedArrayType ArrayType => JSTypedArrayType.BigUInt64;

    private JSBigUInt64Array(JSValue value) => _value = value;

    /// <summary>
    /// Creates a new typed array of specified length, with newly allocated memory.
    /// </summary>
    public JSBigUInt64Array(int length)
    {
        JSValue arrayBuffer = JSValue.CreateArrayBuffer(length * ElementSize);
        _value = JSValue.CreateTypedArray(ArrayType, length, arrayBuffer, 0);
    }

    /// <summary>
    /// Creates a typed-array over memory, without copying.
    /// </summary>
    public unsafe JSBigUInt64Array(Memory<ulong> data)
    {
        JSValue value = JSTypedArrayMemoryManager<ulong>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // The Memory was NOT created from a JS TypedArray. Most likely it was allocated
            // directly or via a .NET array or string.

            JSValue arrayBuffer = data.Length > 0 ?
                JSValue.CreateExternalArrayBuffer(data) : JSValue.CreateArrayBuffer(0);
            _value = JSValue.CreateTypedArray(ArrayType, data.Length, arrayBuffer, 0);
        }
    }

    /// <summary>
    /// Creates a typed-array over read-memory, without copying. Only valid for memory
    /// which was previously marshaled from a JS typed-array to .NET.
    /// </summary>
    /// <exception cref="NotSupportedException">The memory is external to JS.</exception>
    public unsafe JSBigUInt64Array(ReadOnlyMemory<ulong> data)
    {
        JSValue value = JSTypedArrayMemoryManager<ulong>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // Consider copying the memory?
            throw new NotSupportedException(
                "Read-only memory cannot be transferred from .NET to JS.");
        }
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSBigUInt64Array(ulong[] data) : this(data.AsMemory())
    {
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSBigUInt64Array(ulong[] data, int start, int length)
        : this(data.AsMemory().Slice(start, length))
    {
    }

    public int Length => _value.GetTypedArrayLength(out _);

    public ulong this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    /// <summary>
    /// Gets the typed-array values as a span, without copying.
    /// </summary>
    public Span<ulong> Span => _value.GetTypedArrayData<ulong>();

    /// <summary>
    /// Gets the typed-array values as memory, without copying.
    /// </summary>
    public Memory<ulong> Memory => new JSTypedArrayMemoryManager<ulong>(this, Span).Memory;

    /// <summary>
    /// Copies the typed-array data into a new array and returns the array.
    /// </summary>
    public ulong[] ToArray() => Span.ToArray();

    /// <summary>
    /// Copies the typed-array data into an array.
    /// </summary>
    public void CopyTo(ulong[] array, int arrayIndex)
    {
        Span.CopyTo(new Span<ulong>(array, arrayIndex, array.Length - arrayIndex));
    }

    public Span<ulong>.Enumerator GetEnumerator() => Span.GetEnumerator();

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSBigUInt64Array a, JSBigUInt64Array b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSBigUInt64Array a, JSBigUInt64Array b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}

public readonly ref struct JSFloat32Array
{
    private readonly JSValue _value;

    public static explicit operator JSFloat32Array(JSValue value) => new(value);
    public static implicit operator JSValue(JSFloat32Array arr) => arr._value;

    private static int ElementSize => sizeof(float);

    private static JSTypedArrayType ArrayType => JSTypedArrayType.Float32;

    private JSFloat32Array(JSValue value) => _value = value;

    /// <summary>
    /// Creates a new typed array of specified length, with newly allocated memory.
    /// </summary>
    public JSFloat32Array(int length)
    {
        JSValue arrayBuffer = JSValue.CreateArrayBuffer(length * ElementSize);
        _value = JSValue.CreateTypedArray(ArrayType, length, arrayBuffer, 0);
    }

    /// <summary>
    /// Creates a typed-array over memory, without copying.
    /// </summary>
    public unsafe JSFloat32Array(Memory<float> data)
    {
        JSValue value = JSTypedArrayMemoryManager<float>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // The Memory was NOT created from a JS TypedArray. Most likely it was allocated
            // directly or via a .NET array or string.

            JSValue arrayBuffer = data.Length > 0 ?
                JSValue.CreateExternalArrayBuffer(data) : JSValue.CreateArrayBuffer(0);
            _value = JSValue.CreateTypedArray(ArrayType, data.Length, arrayBuffer, 0);
        }
    }

    /// <summary>
    /// Creates a typed-array over read-memory, without copying. Only valid for memory
    /// which was previously marshaled from a JS typed-array to .NET.
    /// </summary>
    /// <exception cref="NotSupportedException">The memory is external to JS.</exception>
    public unsafe JSFloat32Array(ReadOnlyMemory<float> data)
    {
        JSValue value = JSTypedArrayMemoryManager<float>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // Consider copying the memory?
            throw new NotSupportedException(
                "Read-only memory cannot be transferred from .NET to JS.");
        }
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSFloat32Array(float[] data) : this(data.AsMemory())
    {
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSFloat32Array(float[] data, int start, int length)
        : this(data.AsMemory().Slice(start, length))
    {
    }

    public int Length => _value.GetTypedArrayLength(out _);

    public float this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    /// <summary>
    /// Gets the typed-array values as a span, without copying.
    /// </summary>
    public Span<float> Span => _value.GetTypedArrayData<float>();

    /// <summary>
    /// Gets the typed-array values as memory, without copying.
    /// </summary>
    public Memory<float> Memory => new JSTypedArrayMemoryManager<float>(this, Span).Memory;

    /// <summary>
    /// Copies the typed-array data into a new array and returns the array.
    /// </summary>
    public float[] ToArray() => Span.ToArray();

    /// <summary>
    /// Copies the typed-array data into an array.
    /// </summary>
    public void CopyTo(float[] array, int arrayIndex)
    {
        Span.CopyTo(new Span<float>(array, arrayIndex, array.Length - arrayIndex));
    }

    public Span<float>.Enumerator GetEnumerator() => Span.GetEnumerator();

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSFloat32Array a, JSFloat32Array b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSFloat32Array a, JSFloat32Array b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}

public readonly ref struct JSFloat64Array
{
    private readonly JSValue _value;

    public static explicit operator JSFloat64Array(JSValue value) => new(value);
    public static implicit operator JSValue(JSFloat64Array arr) => arr._value;

    private static int ElementSize => sizeof(double);

    private static JSTypedArrayType ArrayType => JSTypedArrayType.Float64;

    private JSFloat64Array(JSValue value) => _value = value;

    /// <summary>
    /// Creates a new typed array of specified length, with newly allocated memory.
    /// </summary>
    public JSFloat64Array(int length)
    {
        JSValue arrayBuffer = JSValue.CreateArrayBuffer(length * ElementSize);
        _value = JSValue.CreateTypedArray(ArrayType, length, arrayBuffer, 0);
    }

    /// <summary>
    /// Creates a typed-array over memory, without copying.
    /// </summary>
    public unsafe JSFloat64Array(Memory<double> data)
    {
        JSValue value = JSTypedArrayMemoryManager<double>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // The Memory was NOT created from a JS TypedArray. Most likely it was allocated
            // directly or via a .NET array or string.

            JSValue arrayBuffer = data.Length > 0 ?
                JSValue.CreateExternalArrayBuffer(data) : JSValue.CreateArrayBuffer(0);
            _value = JSValue.CreateTypedArray(ArrayType, data.Length, arrayBuffer, 0);
        }
    }

    /// <summary>
    /// Creates a typed-array over read-memory, without copying. Only valid for memory
    /// which was previously marshaled from a JS typed-array to .NET.
    /// </summary>
    /// <exception cref="NotSupportedException">The memory is external to JS.</exception>
    public unsafe JSFloat64Array(ReadOnlyMemory<double> data)
    {
        JSValue value = JSTypedArrayMemoryManager<double>.GetJSValueForMemory(data);
        if (!value.IsUndefined())
        {
            _value = value;
        }
        else
        {
            // Consider copying the memory?
            throw new NotSupportedException(
                "Read-only memory cannot be transferred from .NET to JS.");
        }
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSFloat64Array(double[] data) : this(data.AsMemory())
    {
    }

    /// <summary>
    /// Creates a typed-array over an array, without copying.
    /// </summary>
    public JSFloat64Array(double[] data, int start, int length)
        : this(data.AsMemory().Slice(start, length))
    {
    }

    public int Length => _value.GetTypedArrayLength(out _);

    public double this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    /// <summary>
    /// Gets the typed-array values as a span, without copying.
    /// </summary>
    public Span<double> Span => _value.GetTypedArrayData<double>();

    /// <summary>
    /// Gets the typed-array values as memory, without copying.
    /// </summary>
    public Memory<double> Memory => new JSTypedArrayMemoryManager<double>(this, Span).Memory;

    /// <summary>
    /// Copies the typed-array data into a new array and returns the array.
    /// </summary>
    public double[] ToArray() => Span.ToArray();

    /// <summary>
    /// Copies the typed-array data into an array.
    /// </summary>
    public void CopyTo(double[] array, int arrayIndex)
    {
        Span.CopyTo(new Span<double>(array, arrayIndex, array.Length - arrayIndex));
    }

    public Span<double>.Enumerator GetEnumerator() => Span.GetEnumerator();

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator ==(JSFloat64Array a, JSFloat64Array b) => a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public static bool operator !=(JSFloat64Array a, JSFloat64Array b) => !a._value.StrictEquals(b);

    /// <summary>
    /// Compares two JS values using JS "strict" equality.
    /// </summary>
    public bool Equals(JSValue other) => _value.StrictEquals(other);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is JSReference other && Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            "Hashing JS values is not supported. Use JSSet or JSMap instead.");
    }
}
/// <summary>
/// Holds a reference to a typed-array value until the memory is disposed.
/// </summary>
file unsafe class JSTypedArrayMemoryManager<T> : MemoryManager<T>
{
    private readonly void* _pointer;
    private readonly int _length;
    private readonly JSReference _typedArrayReference;

    public JSTypedArrayMemoryManager(JSValue typedArray, Span<T> data)
    {
        _pointer = Unsafe.AsPointer(ref MemoryMarshal.GetReference(data));
        _length = data.Length;
        _typedArrayReference = new JSReference(typedArray);
    }

    public JSValue JSValue => _typedArrayReference.GetValue();
// TODO: (vmoroz)        throw new ObjectDisposedException(nameof(JSTypedArray<T>));

    public override Span<T> GetSpan()
    {
        return new Span<T>(_pointer, _length);
    }

    public override unsafe MemoryHandle Pin(int elementIndex = 0)
    {
        // Do TypedArray or ArrayBuffer support pinning?
        // This code assumes the memory buffer is not movable.
        Span<T> span = GetSpan().Slice(elementIndex);
        void* pointer = Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        return new MemoryHandle(pointer, handle: default, pinnable: this);
    }

    public override void Unpin() { }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _typedArrayReference.Dispose();
        }
    }

    /// <summary>
    /// Checks if this Memory is already owned by a JS TypedArray value, and if so
    /// returns that JS value.
    /// </summary>
    /// <returns>The JS value, or null if the memory is external to JS.</returns>
    public static unsafe JSValue GetJSValueForMemory(ReadOnlyMemory<T> data)
    {
        // This assumes the owner object of a Memory struct is stored as a reference in the
        // first (private) field of the struct. If the Memory internal structure ever changes
        // (in a future major version of the .NET Runtime), this unsafe code could crash.
        // Unfortunately there's no public API to get the Memory owner object.
        void* memoryPointer = Unsafe.AsPointer(ref data);
        object? memoryOwner = Unsafe.Read<object?>(memoryPointer);
        if (memoryOwner is JSTypedArrayMemoryManager<T> manager)
        {
            // The Memory was created from a JS TypedArray.

            // Strip the high bit of the index - it has a special meaning.
            void* memoryIndexPointer = (byte*)memoryPointer + Unsafe.SizeOf<object?>();
            int index = Unsafe.Read<int>(memoryIndexPointer) & ~int.MinValue;

            void* memoryLengthPointer = (byte*)memoryIndexPointer + Unsafe.SizeOf<int>();
            int length = Unsafe.Read<int>(memoryLengthPointer);

            JSValue value = manager.JSValue;
            int valueLength = value.GetTypedArrayLength(out _);

            if (index != 0 || length != valueLength)
            {
                // The Memory was sliced, so get an equivalent slice of the JS TypedArray.
                value = value.CallMethod("slice", index, index + length);
            }

            return value;
        }

        return JSValue.Undefined;
    }
}
