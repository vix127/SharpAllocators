// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace SharpAllocators;

public unsafe interface IAllocator
{
    public T* Allocate<T>(nuint elementCount) where T : unmanaged;
    public void Free<T>(T* pointer) where T : unmanaged;
    public T* Reallocate<T>(T* pointer, nuint elementCount) where T : unmanaged;
}