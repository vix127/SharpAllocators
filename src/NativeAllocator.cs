// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpAllocators;

public readonly unsafe struct NativeAllocator : IAllocator
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Allocate<T>(long count) where T : unmanaged
    {
        return (T*)NativeMemory.Alloc((nuint)(count * sizeof(T)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Free<T>(T* pointer) where T : unmanaged
    {
        NativeMemory.Free(pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Reallocate<T>(T* pointer, long count) where T : unmanaged
    {
        return (T*)NativeMemory.Realloc(pointer, (nuint)count);
    }
}
