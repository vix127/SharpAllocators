// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpAllocators;
using Xunit;

namespace SharpAllocators.Tests;

public unsafe class ArenaAllocatorTests
{
    [Fact]
    public void Create_WithDefaultCapacity_ShouldSucceed()
    {
        using var arena = ArenaAllocator.Create(0);
        Assert.Equal((nuint)4096, arena.CapacityBytes);
        Assert.Equal((nuint)0, arena.AllocatedBytes);
    }

    [Fact]
    public void Create_WithCustomCapacity_ShouldSucceed()
    {
        using var arena = ArenaAllocator.Create(1024);
        Assert.Equal((nuint)1024, arena.CapacityBytes);
        Assert.Equal((nuint)0, arena.AllocatedBytes);
    }

    [Fact]
    public void Allocate_SingleAllocation_ShouldSucceed()
    {
        using var arena = ArenaAllocator.Create(1024);
        var slice = arena.Allocate<int>(10);

        Assert.Equal((nuint)10, slice.Length);
        Assert.NotEqual(IntPtr.Zero, (IntPtr)slice.Pointer);
        Assert.Equal((nuint)(10 * sizeof(int)), slice.ByteLength);
        Assert.Equal((nuint)(10 * sizeof(int)), arena.AllocatedBytes);
    }

    [Fact]
    public void Allocate_MultipleAllocations_ShouldSucceed()
    {
        using var arena = ArenaAllocator.Create(1024);
        var slice1 = arena.Allocate<int>(10);
        var slice2 = arena.Allocate<long>(5);
        var slice3 = arena.Allocate<byte>(100);

        Assert.Equal((nuint)10, slice1.Length);
        Assert.Equal((nuint)5, slice2.Length);
        Assert.Equal((nuint)100, slice3.Length);
        Assert.Equal(
            (nuint)(10 * sizeof(int) + 5 * sizeof(long) + 100 * sizeof(byte)),
            arena.AllocatedBytes);
    }

    [Fact]
    public void Reset_ShouldFreeAllAllocations()
    {
        using var arena = ArenaAllocator.Create(1024);
        arena.Allocate<int>(100);
        arena.Allocate<long>(50);

        Assert.True(arena.AllocatedBytes > 0);

        arena.Reset();

        Assert.Equal((nuint)0, arena.AllocatedBytes);
        Assert.Equal((nuint)1024, arena.CapacityBytes);
    }

    [Fact]
    public void Allocate_AfterReset_ShouldReuseMemory()
    {
        using var arena = ArenaAllocator.Create(1024);
        var slice1 = arena.Allocate<int>(10);
        var firstPointer = (IntPtr)slice1.Pointer;

        arena.Reset();

        var slice2 = arena.Allocate<int>(10);
        var secondPointer = (IntPtr)slice2.Pointer;

        // After reset, the same memory should be reused
        Assert.Equal(firstPointer, secondPointer);
    }

    [Fact]
    public void Allocate_LargeAllocation_ShouldGrowArena()
    {
        using var arena = ArenaAllocator.Create(64); // Very small initial capacity
        var slice = arena.Allocate<int>(1000); // Needs 4000 bytes

        Assert.Equal((nuint)1000, slice.Length);
        Assert.NotEqual(IntPtr.Zero, (IntPtr)slice.Pointer);
        Assert.True(arena.CapacityBytes >= 4000);
    }

    [Fact]
    public void Dispose_ShouldFreeMemory()
    {
        var arena = ArenaAllocator.Create(1024);
        arena.Allocate<int>(100);
        var allocatedBefore = arena.AllocatedBytes;

        Assert.True(allocatedBefore > 0);

        arena.Dispose();

        // After dispose, accessing the arena is undefined behavior,
        // but we can at least verify the method runs without throwing
    }

    [Fact]
    public void Free_IsNoOp()
    {
        using var arena = ArenaAllocator.Create(1024);
        var slice = arena.Allocate<int>(10);
        var allocatedBefore = arena.AllocatedBytes;

        arena.Free(slice.Pointer);

        // Free should be a no-op for arena allocator
        Assert.Equal(allocatedBefore, arena.AllocatedBytes);
    }

    [Fact]
    public void Reallocate_ShouldAllocateNewMemory()
    {
        using var arena = ArenaAllocator.Create(1024);
        var slice1 = arena.Allocate<int>(10);
        var originalPointer = (IntPtr)slice1.Pointer;

        var slice2 = arena.Reallocate<int>(slice1.Pointer, 20);

        Assert.Equal((nuint)20, slice2.Length);
        Assert.NotEqual(IntPtr.Zero, (IntPtr)slice2.Pointer);
        // Reallocate in arena creates new allocation, doesn't extend in place
        Assert.NotEqual(originalPointer, (IntPtr)slice2.Pointer);
    }

    [Fact]
    public void Allocate_VariousTypes_ShouldWork()
    {
        using var arena = ArenaAllocator.Create(4096);

        var intSlice = arena.Allocate<int>(10);
        var longSlice = arena.Allocate<long>(10);
        var doubleSlice = arena.Allocate<double>(10);
        var byteSlice = arena.Allocate<byte>(100);

        Assert.Equal((nuint)10, intSlice.Length);
        Assert.Equal((nuint)10, longSlice.Length);
        Assert.Equal((nuint)10, doubleSlice.Length);
        Assert.Equal((nuint)100, byteSlice.Length);
    }

    [Fact]
    public void AllocatedBytes_ShouldTrackCorrectly()
    {
        using var arena = ArenaAllocator.Create(4096);

        Assert.Equal((nuint)0, arena.AllocatedBytes);

        arena.Allocate<int>(10); // 40 bytes
        Assert.Equal((nuint)40, arena.AllocatedBytes);

        arena.Allocate<long>(5); // 40 bytes
        Assert.Equal((nuint)80, arena.AllocatedBytes);

        arena.Allocate<byte>(100); // 100 bytes
        Assert.Equal((nuint)180, arena.AllocatedBytes);
    }
}