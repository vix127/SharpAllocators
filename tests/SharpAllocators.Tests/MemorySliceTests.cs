// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using SharpAllocators;
using Xunit;

namespace SharpAllocators.Tests;

public unsafe class MemorySliceTests
{
    [Fact]
    public void Constructor_WithPointerAndLength_ShouldCreateSlice()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        var memorySlice = new MemorySlice<int>(slice.Pointer, slice.Length);

        Assert.Equal((IntPtr)slice.Pointer, (IntPtr)memorySlice.Pointer);
        Assert.Equal(slice.Length, memorySlice.Length);
        Assert.Equal(slice.ByteLength, memorySlice.ByteLength);
    }

    [Fact]
    public void Pointer_ShouldReturnCorrectPointer()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        var memorySlice = new MemorySlice<int>(slice.Pointer, slice.Length);

        Assert.Equal((IntPtr)slice.Pointer, (IntPtr)memorySlice.Pointer);
    }

    [Fact]
    public void Length_ShouldReturnCorrectLength()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        var memorySlice = new MemorySlice<int>(slice.Pointer, slice.Length);

        Assert.Equal((nuint)10, memorySlice.Length);
    }

    [Fact]
    public void ByteLength_ShouldReturnCorrectByteLength()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10); // 40 bytes on 64-bit

        var memorySlice = new MemorySlice<int>(slice.Pointer, slice.Length);

        Assert.Equal((nuint)(10 * sizeof(int)), memorySlice.ByteLength);
    }

    [Fact]
    public void Deconstruct_ShouldReturnPointerAndLength()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        var memorySlice = new MemorySlice<int>(slice.Pointer, slice.Length);

        memorySlice.Deconstruct(out int* pointer, out nuint length);

        Assert.Equal((IntPtr)slice.Pointer, (IntPtr)pointer);
        Assert.Equal(slice.Length, length);
    }

    [Fact]
    public void MemorySlice_Implements_IMemorySlice()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        var memorySlice = new MemorySlice<int>(slice.Pointer, slice.Length);

        // Verify it implements the interface
        IMemorySlice<int, nuint> iSlice = memorySlice;
        Assert.Equal((IntPtr)slice.Pointer, (IntPtr)iSlice.Pointer);
        Assert.Equal(slice.Length, iSlice.Length);
    }

    [Fact]
    public void MemorySlice_WithDifferentTypes_ShouldWork()
    {
        var allocator = new NativeAllocator();

        var intSlice = new MemorySlice<int>(allocator.Allocate<int>(5).Pointer, 5);
        var longSlice = new MemorySlice<long>(allocator.Allocate<long>(5).Pointer, 5);
        var doubleSlice = new MemorySlice<double>(allocator.Allocate<double>(5).Pointer, 5);
        var byteSlice = new MemorySlice<byte>(allocator.Allocate<byte>(100).Pointer, 100);

        Assert.Equal((nuint)5, intSlice.Length);
        Assert.Equal((nuint)5, longSlice.Length);
        Assert.Equal((nuint)5, doubleSlice.Length);
        Assert.Equal((nuint)100, byteSlice.Length);

        Assert.Equal((nuint)(5 * sizeof(int)), intSlice.ByteLength);
        Assert.Equal((nuint)(5 * sizeof(long)), longSlice.ByteLength);
        Assert.Equal((nuint)(5 * sizeof(double)), doubleSlice.ByteLength);
        Assert.Equal((nuint)(100 * sizeof(byte)), byteSlice.ByteLength);

        allocator.Free(intSlice.Pointer);
        allocator.Free(longSlice.Pointer);
        allocator.Free(doubleSlice.Pointer);
        allocator.Free(byteSlice.Pointer);
    }

    [Fact]
    public void Equality_TwoSlicesWithSamePointerAndLength_ShouldBeEqual()
    {
        var allocator = new NativeAllocator();
        var slice1 = allocator.Allocate<int>(10);
        var slice2 = allocator.Allocate<int>(10);

        // Create MemorySlice with same values
        var memSlice1 = new MemorySlice<int>(slice1.Pointer, slice1.Length);
        var memSlice2 = new MemorySlice<int>(slice1.Pointer, slice1.Length);

        // Since it's a struct, we compare field by field
        Assert.Equal((IntPtr)memSlice1.Pointer, (IntPtr)memSlice2.Pointer);
        Assert.Equal(memSlice1.Length, memSlice2.Length);

        allocator.Free(slice1.Pointer);
        allocator.Free(slice2.Pointer);
    }
}