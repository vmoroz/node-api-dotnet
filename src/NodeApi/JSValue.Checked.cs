// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.JavaScript.NodeApi.Runtime;
using static Microsoft.JavaScript.NodeApi.Runtime.JSRuntime;

namespace Microsoft.JavaScript.NodeApi;

public readonly ref partial struct JSValue
{
    public readonly struct Checked
    {
        private readonly napi_value _handle = default;
        private readonly JSValueScope? _scope = null;

        public readonly JSValueScope Scope => _scope ?? JSValueScope.Current;

        internal JSRuntime Runtime => Scope.Runtime;

        /// <summary>
        /// Creates an empty instance of <see cref="JSValue" />, which implicitly converts to
        /// <see cref="JSValue.Undefined" /> when used in any scope.
        /// </summary>
        public Checked()
        {
            _handle = default;
            _scope = null;
        }

        /// <summary>
        /// Creates a new instance of <see cref="JSValue" /> from a handle in the current scope.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the handle is null.</exception>
        /// <remarks>
        /// WARNING: A JS value handle is a pointer to a location in memory, so an invalid handle here
        /// may cause an attempt to access an invalid memory location.
        /// </remarks>
        public Checked(napi_value handle) : this(handle, JSValueScope.Current) { }

        /// <summary>
        /// Creates a new instance of <see cref="JSValue" /> from a handle in the specified scope.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when either the handle or scope is null
        /// (unless they are both null then this becomes an empty value that implicitly converts
        /// to <see cref="JSValue.Undefined"/>).</exception>
        /// <remarks>
        /// WARNING: A JS value handle is a pointer to a location in memory, so an invalid handle here
        /// may cause an attempt to access an invalid memory location.
        /// </remarks>
        public Checked(napi_value handle, JSValueScope scope)
        {
            if (handle.IsNull) throw new ArgumentNullException(nameof(handle));
            _handle = handle;
            _scope = scope;
        }

        /// <summary>
        /// Creates an empty instance of <see cref="JSValue" />, which implicitly converts to
        /// <see cref="JSValue.Undefined" /> when used in any scope.
        /// </summary>
        public Checked(JSValue value)
        {
            _handle = value.Handle;
            _scope = value.Scope;
        }

        /// <summary>
        /// Gets the value handle, or throws an exception if the value scope is disposed or
        /// access from the current thread is invalid.
        /// </summary>
        /// <exception cref="JSValueScopeClosedException">The scope has been closed.</exception>
        /// <exception cref="JSInvalidThreadAccessException">The scope is not valid on the current
        /// thread.</exception>
        public napi_value Handle
        {
            get
            {
                if (_scope == null)
                {
                    // If the scope is null, this is an empty (uninitialized) instance.
                    // Implicitly convert to the JS `undefined` value.
                    JSValueScope scope = JSValueScope.Current;
                    return scope.Runtime.GetUndefined(
                        scope.UncheckedEnvironmentHandle, out napi_value result)
                        .ThrowIfFailed(result);
                }

                // Ensure the scope is valid and on the current thread (environment).
                _scope.ThrowIfDisposed();
                _scope.ThrowIfInvalidThreadAccess();

                // The handle must be non-null when the scope is non-null.
                return _handle;
            }
        }

        public static implicit operator Checked(JSValue value) => new(value);
        public static implicit operator Checked?(napi_value handle)
            => handle.IsNull ? null : new(handle);

        public static explicit operator JSValue(Checked value)
            => new JSValue(value.Handle, value.Scope);

        public static explicit operator JSValue(Checked? value)
            => value is Checked nonNullValue
               ? new JSValue(nonNullValue.Handle, nonNullValue.Scope)
               : JSValue.Undefined;

        public static explicit operator napi_value(Checked? value)
            => value.HasValue ? value.Value.Handle : napi_value.Null;

        internal napi_env UncheckedEnvironmentHandle => Scope.UncheckedEnvironmentHandle;

        public JSValue ToValue()
        {
            if (_scope == null)
            {
                return default;
            }

            // Ensure the scope is valid and on the current thread (environment).
            _scope.ThrowIfDisposed();
            _scope.ThrowIfInvalidThreadAccess();

            // The handle must be non-null when the scope is non-null.
            return new JSValue(_handle, _scope);
        }

        public JSFunction AsFunction() => new(ToValue());

        public JSObject AsObject() => new(ToValue());

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            // JSValue cannot be boxed.
            return false;
        }

        public override int GetHashCode()
        {
            throw new NotSupportedException(
                "Hashing JS values is not supported. Use JSSet or JSMap instead.");
        }

        public static implicit operator Checked(bool value) => JSValue.GetBoolean(value);
        public static implicit operator Checked(sbyte value) => JSValue.CreateNumber(value);
        public static implicit operator Checked(byte value) => JSValue.CreateNumber(value);
        public static implicit operator Checked(short value) => JSValue.CreateNumber(value);
        public static implicit operator Checked(ushort value) => JSValue.CreateNumber(value);
        public static implicit operator Checked(int value) => JSValue.CreateNumber(value);
        public static implicit operator Checked(uint value) => JSValue.CreateNumber(value);
        public static implicit operator Checked(long value) => JSValue.CreateNumber(value);
        public static implicit operator Checked(ulong value) => JSValue.CreateNumber(value);
        public static implicit operator Checked(float value) => JSValue.CreateNumber(value);
        public static implicit operator Checked(double value) => JSValue.CreateNumber(value);
        //public static implicit operator JSValue.Checked(bool? value) => JSValue.ValueOrDefault(value, value => GetBoolean(value));
        //public static implicit operator JSValue.Checked(sbyte? value) => ValueOrDefault(value, value => CreateNumber(value));
        //public static implicit operator JSValue.Checked(byte? value) => ValueOrDefault(value, value => CreateNumber(value));
        //public static implicit operator JSValue.Checked(short? value) => ValueOrDefault(value, value => CreateNumber(value));
        //public static implicit operator JSValue.Checked(ushort? value) => ValueOrDefault(value, value => CreateNumber(value));
        //public static implicit operator JSValue.Checked(int? value) => ValueOrDefault(value, value => CreateNumber(value));
        //public static implicit operator JSValue.Checked(uint? value) => ValueOrDefault(value, value => CreateNumber(value));
        //public static implicit operator JSValue.Checked(long? value) => ValueOrDefault(value, value => CreateNumber(value));
        //public static implicit operator JSValue.Checked(ulong? value) => ValueOrDefault(value, value => CreateNumber(value));
        //public static implicit operator JSValue.Checked(float? value) => ValueOrDefault(value, value => CreateNumber(value));
        //public static implicit operator JSValue.Checked(double? value) => ValueOrDefault(value, value => CreateNumber(value));
        public static implicit operator Checked(string value) => value == null ? default : JSValue.CreateStringUtf16(value);
        public static implicit operator Checked(char[] value) => value == null ? default : JSValue.CreateStringUtf16(value);
        public static implicit operator Checked(Span<char> value) => JSValue.CreateStringUtf16(value);
        public static implicit operator Checked(ReadOnlySpan<char> value) => JSValue.CreateStringUtf16(value);
        public static implicit operator Checked(byte[] value) => value == null ? default : JSValue.CreateStringUtf8(value);
        public static implicit operator Checked(Span<byte> value) => JSValue.CreateStringUtf8(value);
        public static implicit operator Checked(ReadOnlySpan<byte> value) => JSValue.CreateStringUtf8(value);
    }
}
