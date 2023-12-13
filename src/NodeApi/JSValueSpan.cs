// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Microsoft.JavaScript.NodeApi.Runtime.JSRuntime;

namespace Microsoft.JavaScript.NodeApi;

public readonly ref struct JSValueSpan
{
    private readonly Span<napi_value> _span;

    internal JSValueSpan(JSValueScope scope, Span<napi_value> span)
    {
        Scope = scope;
        _span = span;
    }

    internal JSValueSpan(Span<napi_value> span)
    {
        Scope = JSValueScope.Current;
        _span = span;
    }

    internal JSValueScope Scope { get; }

    public JSValue this[int index]
    {
        get => index < _span.Length ? new JSValue(_span[index], Scope) : default;
        set => _span[index] = value.Handle;
    }

    public int Length => _span.Length;

    public bool IsEmpty => _span.IsEmpty;

    public static bool operator !=(JSValueSpan left, JSValueSpan right) => !(left == right);

    public static bool operator ==(JSValueSpan left, JSValueSpan right)
        => left.Scope == right.Scope && left._span == right._span;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member.
    [Obsolete("Equals() on JSValueSpan will always throw an exception. "
        + "Use the equality operator instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) =>
        throw new NotSupportedException("Not supported");

    [Obsolete("GetHashCode() on Span will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() =>
        throw new NotSupportedException("Not supported");
#pragma warning restore CS0809

    public void CopyTo(JSValueSpan destination) => _span.CopyTo(destination._span);

    /// <summary>
    /// Copies all elements of this span into a destination span, starting at the specified index.
    /// </summary>
    public void CopyTo(JSValueSpan destination, int destinationIndex)
        => _span.CopyTo(destination._span.Slice(destinationIndex));

    public static JSValueSpan Empty => default;

    /// <summary>Gets an enumerator for this span.</summary>
    public Enumerator GetEnumerator() => new Enumerator(this);

    public ref struct Enumerator
    {
        private readonly JSValueSpan _span;
        private int _index;

        /// <summary>Initialize the enumerator.</summary>
        /// <param name="span">The span to enumerate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(JSValueSpan span)
        {
            _span = span;
            _index = -1;
        }

        /// <summary>Advances the enumerator to the next element of the span.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            int index = _index + 1;
            if (index < _span.Length)
            {
                _index = index;
                return true;
            }

            return false;
        }

        /// <summary>Gets the element at the current position of the enumerator.</summary>
        public JSValue Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _span[_index];
        }
    }

    public static implicit operator JSValueReadOnlySpan(JSValueSpan valueSpan)
        => new JSValueReadOnlySpan(valueSpan.Scope, valueSpan._span);
}
