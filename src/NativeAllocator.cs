// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpAllocators;

public readonly unsafe struct NativeAllocator : IAllocator
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySlice<T> Allocate<T>(nuint elementCount) where T : unmanaged
    {
        var pointer = (T*)NativeMemory.Alloc(elementCount * (nuint)sizeof(T));

        return new(pointer, elementCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Free<T>(T* pointer) where T : unmanaged
    {
        NativeMemory.Free(pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemorySlice<T> Reallocate<T>(T* pointer, nuint newLength) where T : unmanaged
    {
        var reallocatedPointer = (T*)NativeMemory.Realloc(pointer, newLength * (nuint)sizeof(T));

        return new(reallocatedPointer, newLength);
    }
}
