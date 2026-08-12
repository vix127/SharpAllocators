// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace SharpAllocators;

public unsafe interface IMemorySlice<TPointer, TLength>
    where TPointer : unmanaged
    where TLength  : unmanaged
{
    public TPointer* Pointer { get; }
    public TLength   Length { get; }
}
