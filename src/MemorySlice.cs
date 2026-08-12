// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace SharpAllocators;

public readonly unsafe struct MemorySlice<T> : IMemorySlice<T, nuint>
    where T : unmanaged
{
    public T* Pointer { get; }
    public nuint Length { get; }
    public nuint ByteLength => Length * (nuint)sizeof(T);

    public MemorySlice(T* pointer, nuint length)
    {
        Pointer = pointer;
        Length = length;
    }

    public void Deconstruct(out T* pointer, out nuint lenght)
    {
        pointer = Pointer;
        lenght = Length;
    }
}
