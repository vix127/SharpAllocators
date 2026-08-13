// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace SharpAllocators;

public readonly ref struct LongSpan<T>
{
    private readonly ref T _reference;
    public readonly long Length { get; }
    public readonly bool IsEmpty => Length is 0;
}
