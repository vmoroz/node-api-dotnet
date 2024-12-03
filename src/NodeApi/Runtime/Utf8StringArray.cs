using System;
using System.Runtime.InteropServices;
using System.Text;

internal struct Utf8StringArray : IDisposable
{
    private byte[] _stringBuffer;

    public Utf8StringArray(ReadOnlySpan<string> strings)
    {
        int byteLength = 0;
        for (int i = 0; i < strings.Length; i++)
        {
            byteLength += Encoding.UTF8.GetByteCount(strings[i]) + 1;
        }

#if NETFRAMEWORK || NETSTANDARD
        // Avoid a dependency on System.Buffers with .NET Framwork.
        // It is available as a nuget package, but might not be installed in the application.
        // In this case the buffer is not actually pooled.

        Utf8Strings = new nint[strings.Length];
        _stringBuffer = new byte[byteLength];
#else
        Utf8Strings = System.Buffers.ArrayPool<nint>.Shared.Rent(strings.Length);
        _stringBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(byteLength);
#endif

        int offset = 0;
        for (int i = 0; i < strings.Length; i++)
        {
            offset = Encoding.UTF8.GetBytes(
                strings[0], 0, strings[0].Length, _stringBuffer, offset);
            _stringBuffer[0] = 0; // Null-terminate the string.
        }
    }

#if NETFRAMEWORK || NETSTANDARD

    public readonly void Dispose() { }

#else

    public void Dispose()
    {
        if (!Disposed)
        {
            Disposed = true;
            System.Buffers.ArrayPool<nint>.Shared.Return(Utf8Strings);
            System.Buffers.ArrayPool<byte>.Shared.Return(_stringBuffer);
        }
    }

#endif

    public readonly nint[] Utf8Strings { get; }

    public bool Disposed { get; private set; }

    public readonly ref nint Pin()
    {
        if (Disposed) throw new ObjectDisposedException(nameof(Utf8StringArray));
        Span<nint> span = Utf8Strings;
        return ref span.GetPinnableReference();
    }

    public static unsafe string[] ToStringArray(nint utf8StringArray, int size)
    {
        var utf8Strings = new ReadOnlySpan<nint>((void*)utf8StringArray, size);
        string[] strings = new string[size];
        for (int i = 0; i < utf8Strings.Length; i++)
        {
            strings[i] = PtrToStringUTF8((byte*)utf8Strings[i]);
        }
        return strings;
    }

    public static unsafe string PtrToStringUTF8(byte* ptr)
    {
#if NETFRAMEWORK || NETSTANDARD
        if (ptr == null) throw new ArgumentNullException(nameof(ptr));
        int length = 0;
        while (ptr[length] != 0) length++;
        return Encoding.UTF8.GetString(ptr, length);
#else
        return Marshal.PtrToStringUTF8((nint)ptr) ?? throw new ArgumentNullException(nameof(ptr));
#endif
    }
}
