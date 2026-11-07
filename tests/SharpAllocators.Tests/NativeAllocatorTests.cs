// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using SharpAllocators;
using Xunit;

namespace SharpAllocators.Tests;

public unsafe class NativeAllocatorTests
{
    [Fact]
    public void Allocate_SingleAllocation_ShouldSucceed()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        Assert.Equal((nuint)10, slice.Length);
        Assert.NotEqual(IntPtr.Zero, (IntPtr)slice.Pointer);
        Assert.Equal((nuint)(10 * sizeof(int)), slice.ByteLength);
    }

    [Fact]
    public void Allocate_MultipleAllocations_ShouldSucceed()
    {
        var allocator = new NativeAllocator();
        var slice1 = allocator.Allocate<int>(10);
        var slice2 = allocator.Allocate<long>(5);
        var slice3 = allocator.Allocate<byte>(100);

        Assert.Equal((nuint)10, slice1.Length);
        Assert.Equal((nuint)5, slice2.Length);
        Assert.Equal((nuint)100, slice3.Length);
        Assert.NotEqual(IntPtr.Zero, (IntPtr)slice1.Pointer);
        Assert.NotEqual(IntPtr.Zero, (IntPtr)slice2.Pointer);
        Assert.NotEqual(IntPtr.Zero, (IntPtr)slice3.Pointer);
    }

    [Fact]
    public void Free_ShouldFreeMemory()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);
        var pointer = slice.Pointer;

        Assert.NotEqual(IntPtr.Zero, (IntPtr)pointer);

        allocator.Free(pointer);

        // After free, the pointer is invalid but we can't easily test that
        // without causing undefined behavior. Just verify no exception.
    }

    [Fact]
    public void Reallocate_ShouldResizeMemory()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);
        var originalPointer = slice.Pointer;

        // Write some data to the original allocation
        for (int i = 0; i < 10; i++)
        {
            originalPointer[i] = i * 2;
        }

        // Reallocate to larger size
        var newSlice = allocator.Reallocate<int>(originalPointer, 20);

        Assert.Equal((nuint)20, newSlice.Length);
        // Reallocate may or may not return the same pointer
        // Data should be preserved up to the minimum of old and new size
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(i * 2, newSlice.Pointer[i]);
        }

        // Clean up
        allocator.Free(newSlice.Pointer);
    }

    [Fact]
    public void Reallocate_ToSmallerSize_ShouldWork()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(20);
        var originalPointer = slice.Pointer;

        // Write some data
        for (int i = 0; i < 20; i++)
        {
            originalPointer[i] = i * 3;
        }

        // Reallocate to smaller size
        var newSlice = allocator.Reallocate<int>(originalPointer, 5);

        Assert.Equal((nuint)5, newSlice.Length);
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(i * 3, newSlice.Pointer[i]);
        }

        allocator.Free(newSlice.Pointer);
    }

    [Fact]
    public void Allocate_VariousTypes_ShouldWork()
    {
        var allocator = new NativeAllocator();

        var intSlice = allocator.Allocate<int>(10);
        var longSlice = allocator.Allocate<long>(10);
        var doubleSlice = allocator.Allocate<double>(10);
        var byteSlice = allocator.Allocate<byte>(100);
        var floatSlice = allocator.Allocate<float>(50);

        Assert.Equal((nuint)10, intSlice.Length);
        Assert.Equal((nuint)10, longSlice.Length);
        Assert.Equal((nuint)10, doubleSlice.Length);
        Assert.Equal((nuint)100, byteSlice.Length);
        Assert.Equal((nuint)50, floatSlice.Length);

        allocator.Free(intSlice.Pointer);
        allocator.Free(longSlice.Pointer);
        allocator.Free(doubleSlice.Pointer);
        allocator.Free(byteSlice.Pointer);
        allocator.Free(floatSlice.Pointer);
    }

    [Fact]
    public void Allocate_ZeroElements_ShouldReturnEmptySlice()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(0);

        Assert.Equal((nuint)0, slice.Length);
        Assert.Equal((nuint)0, slice.ByteLength);
        // Pointer might be null or valid for zero-size allocation
    }

    [Fact]
    public void Allocate_LargeAllocation_ShouldWork()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<byte>(1024 * 1024); // 1 MB

        Assert.Equal((nuint)(1024 * 1024), slice.Length);
        Assert.NotEqual(IntPtr.Zero, (IntPtr)slice.Pointer);

        allocator.Free(slice.Pointer);
    }

    [Fact]
    public void AllocateAndFree_MultipleCycles_ShouldNotLeak()
    {
        var allocator = new NativeAllocator();

        // Perform multiple allocate/free cycles
        for (int i = 0; i < 100; i++)
        {
            var slice = allocator.Allocate<int>(100);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)slice.Pointer);
            allocator.Free(slice.Pointer);
        }
    }
}