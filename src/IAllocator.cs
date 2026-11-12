// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace SharpAllocators;

/// <summary>
/// Defines a contract for memory allocators that manage allocation, deallocation,
/// and reallocation of unmanaged memory blocks.
/// </summary>
/// <remarks>
/// <para>
/// Implementations of this interface provide low-level memory management capabilities
/// suitable for high-performance scenarios where direct control over memory layout
/// and lifetime is required.
/// </para>
/// <para>
/// This interface operates exclusively with unmanaged types (<c>where T : unmanaged</c>)
/// and uses <see cref="nuint"/> for element counts to support large allocations
/// on 64-bit platforms.
/// </para>
/// </remarks>
public unsafe interface IAllocator
{
    /// <summary>
    /// Allocates a contiguous block of unmanaged memory for the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The unmanaged type of elements to allocate. Must be an unmanaged type (primitive, enum, pointer, or struct containing only unmanaged fields).</typeparam>
    /// <param name="elementCount">The number of elements of type <typeparamref name="T"/> to allocate. Must be greater than zero.</param>
    /// <returns>
    /// A <see cref="MemorySlice{T}"/> representing the allocated memory block.
    /// The returned slice contains a valid pointer to the allocated memory and the element count.
    /// </returns>
    /// <exception cref="OutOfMemoryException">Thrown when the requested allocation cannot be satisfied.</exception>
    public MemorySlice<T> Allocate<T>(nuint elementCount) where T : unmanaged;

    /// <summary>
    /// Frees a previously allocated block of unmanaged memory.
    /// </summary>
    /// <typeparam name="T">The unmanaged type of elements that were allocated. Must match the type used during allocation.</typeparam>
    /// <param name="pointer">A pointer to the memory block to free. Must be a pointer returned by a previous call to <see cref="Allocate{T}(nuint)"/> or <see cref="Reallocate{T}(T*, nuint)"/> on the same allocator instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pointer"/> is null on some implementations.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pointer was not allocated by this allocator or has already been freed.</exception>
    /// <remarks>
    /// <para>
    /// Implementations might handle null pointers gracefully (no-op),
    /// but this behavior is not guaranteed across all implementations.
    /// </para>
    /// </remarks>
    public void Free<T>(T* pointer) where T : unmanaged;

    /// <summary>
    /// Resizes a previously allocated memory block to a new element count.
    /// </summary>
    /// <typeparam name="T">The unmanaged type of elements in the memory block. Must match the type used during the original allocation.</typeparam>
    /// <param name="pointer">A pointer to the memory block to resize. Must be a pointer returned by a previous call to <see cref="Allocate{T}(nuint)"/> or <see cref="Reallocate{T}(T*, nuint)"/> on the same allocator instance.</param>
    /// <param name="elementCount">The new number of elements of type <typeparamref name="T"/> to allocate. Must be greater than zero.</param>
    /// <returns>
    /// A <see cref="MemorySlice{T}"/> representing the resized memory block.
    /// The returned pointer may be the same as the input <paramref name="pointer"/> (if resized in-place)
    /// or a new pointer (if the block was moved to a new location).
    /// </returns>
    /// <exception cref="OutOfMemoryException">Thrown when the requested reallocation cannot be satisfied.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pointer"/> is null on some implementations.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pointer was not allocated by this allocator or has already been freed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="elementCount"/> is zero on some implementations.</exception>
    /// <remarks>
    /// <para>
    /// If the new size is larger than the original, the additional memory is uninitialized.
    /// If the new size is smaller, the excess memory is freed and its contents are lost.
    /// </para>
    /// <para>
    /// When the memory block is moved to a new location, the contents of the original
    /// block are copied to the new location up to the minimum of the old and new sizes.
    /// </para>
    /// <para>
    /// The original <paramref name="pointer"/> and any <see cref="MemorySlice{T}"/>
    /// referencing the original memory become invalid after this call. The caller
    /// must use the returned <see cref="MemorySlice{T}"/> for subsequent operations.
    /// </para>
    /// </remarks>
    public MemorySlice<T> Reallocate<T>(T* pointer, nuint elementCount) where T : unmanaged;
}
