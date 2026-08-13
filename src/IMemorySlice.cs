// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace SharpAllocators;

public unsafe interface IMemorySlice<TElement, TLength>
    where TElement : unmanaged
    where TLength  : unmanaged
{
    public TElement* Pointer { get; }
    public TLength   Length { get; }
}
