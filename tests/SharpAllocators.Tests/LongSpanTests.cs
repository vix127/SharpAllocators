// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpAllocators;
using Xunit;

namespace SharpAllocators.Tests;

public unsafe class LongSpanTests
{
    [Fact]
    public void Constructor_WithReference_ShouldCreateSpanOfLength1()
    {
        int value = 42;
        var span = new LongSpan<int>(ref value);

        Assert.Equal(1L, span.Length);
        Assert.False(span.IsEmpty);
        Assert.Equal(42, span[0]);
    }

    [Fact]
    public void Constructor_WithPointerAndLength_ShouldCreateSpan()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        // Initialize with some values
        for (int i = 0; i < 10; i++)
        {
            slice.Pointer[i] = i * 10;
        }

        var span = new LongSpan<int>(slice.Pointer, 10);

        Assert.Equal(10L, span.Length);
        Assert.False(span.IsEmpty);

        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(i * 10, span[i]);
        }

        allocator.Free(slice.Pointer);
    }

    [Fact]
    public void Constructor_WithZeroLength_ShouldCreateEmptySpan()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        var span = new LongSpan<int>(slice.Pointer, 0);

        Assert.Equal(0L, span.Length);
        Assert.True(span.IsEmpty);

        allocator.Free(slice.Pointer);
    }

    [Fact]
    public void Constructor_WithNegativeLength_ShouldThrow()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        var exceptionThrown = false;
        try
        {
            _ = new LongSpan<int>(slice.Pointer, -1);
        }
        catch (ArgumentOutOfRangeException)
        {
            exceptionThrown = true;
        }

        Assert.True(exceptionThrown);
        allocator.Free(slice.Pointer);
    }

    [Fact]
    public void Indexer_ShouldReturnCorrectValues()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(5);

        slice.Pointer[0] = 100;
        slice.Pointer[1] = 200;
        slice.Pointer[2] = 300;
        slice.Pointer[3] = 400;
        slice.Pointer[4] = 500;

        var span = new LongSpan<int>(slice.Pointer, 5);

        Assert.Equal(100, span[0]);
        Assert.Equal(200, span[1]);
        Assert.Equal(300, span[2]);
        Assert.Equal(400, span[3]);
        Assert.Equal(500, span[4]);

        allocator.Free(slice.Pointer);
    }

    [Fact]
    public void Indexer_ShouldAllowModification()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(5);

        var span = new LongSpan<int>(slice.Pointer, 5);

        span[0] = 100;
        span[1] = 200;
        span[2] = 300;

        Assert.Equal(100, slice.Pointer[0]);
        Assert.Equal(200, slice.Pointer[1]);
        Assert.Equal(300, slice.Pointer[2]);

        allocator.Free(slice.Pointer);
    }

    [Fact]
    public void Indexer_OutOfBounds_Get_ShouldThrow()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(5);

        var span = new LongSpan<int>(slice.Pointer, 5);

        // Test negative index
        var exceptionThrown = false;
        try { _ = span[-1]; } catch (IndexOutOfRangeException) { exceptionThrown = true; }
        Assert.True(exceptionThrown);

        // Test index equal to length
        exceptionThrown = false;
        try { _ = span[5]; } catch (IndexOutOfRangeException) { exceptionThrown = true; }
        Assert.True(exceptionThrown);

        // Test index greater than length
        exceptionThrown = false;
        try { _ = span[100]; } catch (IndexOutOfRangeException) { exceptionThrown = true; }
        Assert.True(exceptionThrown);

        allocator.Free(slice.Pointer);
    }

    [Fact]
    public void Indexer_OutOfBounds_Set_ShouldThrow()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(5);

        var span = new LongSpan<int>(slice.Pointer, 5);

        // Test negative index
        var exceptionThrown = false;
        try { span[-1] = 0; } catch (IndexOutOfRangeException) { exceptionThrown = true; }
        Assert.True(exceptionThrown);

        // Test index equal to length
        exceptionThrown = false;
        try { span[5] = 0; } catch (IndexOutOfRangeException) { exceptionThrown = true; }
        Assert.True(exceptionThrown);

        // Test index greater than length
        exceptionThrown = false;
        try { span[100] = 0; } catch (IndexOutOfRangeException) { exceptionThrown = true; }
        Assert.True(exceptionThrown);

        allocator.Free(slice.Pointer);
    }

    [Fact]
    public void LongSpan_WithLargeLength_ShouldWork()
    {
        var allocator = new NativeAllocator();
        // Allocate enough for a large span
        var slice = allocator.Allocate<int>(10000);

        var span = new LongSpan<int>(slice.Pointer, 10000);

        Assert.Equal(10000L, span.Length);
        Assert.False(span.IsEmpty);

        // Test accessing elements at various positions
        span[0] = 1;
        span[5000] = 5000;
        span[9999] = 9999;

        Assert.Equal(1, span[0]);
        Assert.Equal(5000, span[5000]);
        Assert.Equal(9999, span[9999]);

        allocator.Free(slice.Pointer);
    }

    [Fact]
    public void LongSpan_WithDifferentTypes_ShouldWork()
    {
        var allocator = new NativeAllocator();

        var intSlice = allocator.Allocate<int>(10);
        var longSlice = allocator.Allocate<long>(10);
        var doubleSlice = allocator.Allocate<double>(10);

        var intSpan = new LongSpan<int>(intSlice.Pointer, 10);
        var longSpan = new LongSpan<long>(longSlice.Pointer, 10);
        var doubleSpan = new LongSpan<double>(doubleSlice.Pointer, 10);

        intSpan[0] = 42;
        longSpan[0] = 42L;
        doubleSpan[0] = 42.0;

        Assert.Equal(42, intSpan[0]);
        Assert.Equal(42L, longSpan[0]);
        Assert.Equal(42.0, doubleSpan[0]);

        allocator.Free(intSlice.Pointer);
        allocator.Free(longSlice.Pointer);
        allocator.Free(doubleSlice.Pointer);
    }

    [Fact]
    public void LongSpan_IsRefStruct_ShouldWorkOnStack()
    {
        // This test verifies LongSpan is a ref struct and can only be used on stack
        // We can't directly test this at runtime, but we can verify it compiles
        // and works when used as a local variable

        int value = 100;
        var span = new LongSpan<int>(ref value);

        Assert.Equal(1L, span.Length);
        Assert.Equal(100, span[0]);
    }

    [Fact]
    public void LongSpan_Length_ShouldBeLong()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(int.MaxValue);

        var span = new LongSpan<int>(slice.Pointer, (long)int.MaxValue);

        // Length is long, can exceed int.MaxValue
        Assert.Equal((long)int.MaxValue, span.Length);

        allocator.Free(slice.Pointer);
    }

    [Fact]
    public void LongSpan_Empty_ShouldHaveLengthZero()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        var span = new LongSpan<int>(slice.Pointer, 0);

        Assert.Equal(0L, span.Length);
        Assert.True(span.IsEmpty);

        allocator.Free(slice.Pointer);
    }
}