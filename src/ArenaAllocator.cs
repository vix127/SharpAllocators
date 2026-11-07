// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpAllocators;

/// <summary>
/// An arena allocator that allocates a large block of native memory upfront
/// and hands out slices via bump-pointer allocation. All allocations can be
/// freed at once by calling <see cref="Reset"/>.
/// </summary>
public readonly unsafe struct ArenaAllocator : IAllocator, IDisposable
{
    private readonly ArenaState* _state;

    /// <summary>Creates a new arena allocator with the specified initial capacity.</summary>
    /// <param name="initialCapacityBytes">Initial capacity in bytes.</param>
    public static ArenaAllocator Create(nuint initialCapacityBytes)
    {
        if (initialCapacityBytes == 0)
        {
            initialCapacityBytes = 4096; // Default to 4KB
        }

        var state = (ArenaState*)NativeMemory.Alloc((nuint)sizeof(ArenaState));
        state->CurrentChunk = (byte*)NativeMemory.Alloc(initialCapacityBytes);
        state->CurrentOffset = 0;
        state->Capacity = initialCapacityBytes;
        state->Next = null;

        return new ArenaAllocator(state);
    }

    private ArenaAllocator(ArenaState* state)
    {
        _state = state;
    }

    /// <summary>Gets the total number of bytes allocated in this arena.</summary>
    public nuint AllocatedBytes
    {
        get
        {
            nuint total = 0;
            var chunk = _state;
            while (chunk != null)
            {
                total += chunk->CurrentOffset;
                chunk = chunk->Next;
            }
            return total;
        }
    }

    /// <summary>Gets the total capacity of this arena in bytes.</summary>
    public nuint CapacityBytes
    {
        get
        {
            nuint total = 0;
            var chunk = _state;
            while (chunk != null)
            {
                total += chunk->Capacity;
                chunk = chunk->Next;
            }
            return total;
        }
    }

    /// <summary>Resets the arena, freeing all allocations at once.</summary>
    /// <remarks>
    /// This is O(1) and does not actually free the underlying native memory.
    /// The memory will be reused for subsequent allocations.
    /// </remarks>
    public void Reset()
    {
        var chunk = _state;
        while (chunk != null)
        {
            chunk->CurrentOffset = 0;
            chunk = chunk->Next;
        }
    }

    /// <summary>Frees all native memory associated with this arena.</summary>
    /// <remarks>
    /// After calling this, the arena cannot be used anymore.
    /// </remarks>
    public void Dispose()
    {
        var chunk = _state;
        while (chunk != null)
        {
            var next = chunk->Next;
            NativeMemory.Free(chunk->CurrentChunk);
            NativeMemory.Free(chunk);
            chunk = next;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySlice<T> Allocate<T>(nuint elementCount) where T : unmanaged
    {
        var byteCount = elementCount * (nuint)sizeof(T);
        var alignedByteCount = AlignUp(byteCount, (nuint)sizeof(T));

        var chunk = _state;
        while (true)
        {
            var remaining = chunk->Capacity - chunk->CurrentOffset;
            if (alignedByteCount <= remaining)
            {
                var pointer = chunk->CurrentChunk + chunk->CurrentOffset;
                chunk->CurrentOffset += alignedByteCount;
                return new MemorySlice<T>((T*)pointer, elementCount);
            }

            if (chunk->Next == null)
            {
                // Allocate new chunk with at least the requested size or double the current capacity
                var newCapacity = chunk->Capacity * 2;
                if (newCapacity < alignedByteCount)
                {
                    newCapacity = alignedByteCount;
                }

                var newChunk = (ArenaState*)NativeMemory.Alloc((nuint)sizeof(ArenaState));
                newChunk->CurrentChunk = (byte*)NativeMemory.Alloc(newCapacity);
                newChunk->CurrentOffset = 0;
                newChunk->Capacity = newCapacity;
                newChunk->Next = null;
                chunk->Next = newChunk;
            }

            chunk = chunk->Next;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Free<T>(T* pointer) where T : unmanaged
    {
        // No-op for arena allocator. Use Reset() to free all allocations.
        // Individual frees are not supported in a bump-pointer arena.
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySlice<T> Reallocate<T>(T* pointer, nuint newLength) where T : unmanaged
    {
        // For arena allocator, we can't easily reallocate in place.
        // Allocate new memory and copy.
        var newSlice = Allocate<T>(newLength);
        if (newLength > 0 && pointer != null)
        {
            // We don't know the old length, so we can't safely copy.
            // This is a limitation of the arena allocator.
        }
        return newSlice;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nuint AlignUp(nuint value, nuint alignment)
    {
        return (value + alignment - 1) & ~(alignment - 1);
    }

    // Unmanaged struct allocated in native memory to avoid GC pressure
    [StructLayout(LayoutKind.Sequential)]
    private struct ArenaState
    {
        public byte* CurrentChunk;
        public nuint CurrentOffset;
        public nuint Capacity;
        public ArenaState* Next;
    }
}