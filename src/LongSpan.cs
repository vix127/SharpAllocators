// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace SharpAllocators;

/// <summary>
/// <see cref="LongSpan{T}"/> represents a contiguous region of arbitrary memory. Unlike arrays, it can point to either managed
/// or native memory, or to memory allocated on the stack. It is type-safe and memory-safe.
/// </summary>
public readonly ref struct LongSpan<T>
{
    /// <summary>A byref or a native ptr.</summary>
    private readonly ref T _reference;

    /// <summary>The number of elements this Span contains.</summary>
    private readonly long _length;

    /// <summary>
    /// The number of items in the span.
    /// </summary>
    public readonly long Length => _length;

    /// <summary>
    /// Gets a value indicating whether this <see cref="LongSpan{T}"/> is empty.
    /// </summary>
    /// <value><see langword="true"/> if this span is empty; otherwise, <see langword="false"/>.</value>
    public readonly bool IsEmpty => Length is 0;

    /// <summary>Creates a new <see cref="LongSpan{T}"/> of length 1 around the specified reference.</summary>
    /// <param name="reference">A reference to data.</param>
    public LongSpan(ref T reference)
    {
        _reference = ref reference;
        _length = 1;
    }

    /// <summary>
    /// Creates a new span over the target unmanaged buffer.  Clearly this
    /// is quite dangerous, because we are creating arbitrarily typed T's
    /// out of a void*-typed block of memory.  And the length is not checked.
    /// But if this creation is correct, then all subsequent uses are correct.
    /// </summary>
    /// <param name="pointer">An unmanaged pointer to memory.</param>
    /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <typeparamref name="T"/> is reference type or contains pointers and hence cannot be stored in unmanaged memory.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified <paramref name="length"/> is negative.
    /// </exception>
    public unsafe LongSpan(void* pointer, long length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot negative");
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            throw new ArgumentException("Generic type parameter T cannot be a refrence type or contain refrences", nameof(T));
        }

        _reference = ref *(T*)pointer;
        _length = length;
    }

    /// <summary>
    /// Returns a reference to specified element of the Span.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when index less than 0 or index greater than or equal to Length
    /// </exception>
    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= (uint)_length)
            {
                throw new IndexOutOfRangeException("Index cannot be less then zero or greater then or equal to length");
            }

            return ref Unsafe.Add(ref _reference, (nint)(uint)index);
        }
    }
}
