// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using SharpAllocators;

namespace SharpAllocators.Examples;

/// <summary>
/// Example program demonstrating how to use the SharpAllocators library.
/// </summary>
public static unsafe class Program
{
    public static void Main()
    {
        Console.WriteLine("=== SharpAllocators Examples ===\n");

        // Example 1: NativeAllocator - basic allocation and freeing
        Console.WriteLine("--- Example 1: NativeAllocator Basic Usage ---");
        NativeAllocatorBasicExample();

        // Example 2: NativeAllocator - reallocation
        Console.WriteLine("\n--- Example 2: NativeAllocator Reallocation ---");
        NativeAllocatorReallocateExample();

        // Example 3: ArenaAllocator - bump-pointer allocation
        Console.WriteLine("\n--- Example 3: ArenaAllocator Basic Usage ---");
        ArenaAllocatorBasicExample();

        // Example 4: ArenaAllocator - reset to free all at once
        Console.WriteLine("\n--- Example 4: ArenaAllocator Reset ---");
        ArenaAllocatorResetExample();

        // Example 5: ArenaAllocator - automatic growth
        Console.WriteLine("\n--- Example 5: ArenaAllocator Automatic Growth ---");
        ArenaAllocatorGrowthExample();

        // Example 6: MemorySlice - working with memory slices
        Console.WriteLine("\n--- Example 6: MemorySlice ---");
        MemorySliceExample();

        // Example 7: LongSpan - type-safe memory spanning
        Console.WriteLine("\n--- Example 7: LongSpan ---");
        LongSpanExample();

        // Example 8: Using allocators with different types
        Console.WriteLine("\n--- Example 8: Multiple Types ---");
        MultipleTypesExample();

        Console.WriteLine("\n=== All examples completed successfully! ===");
    }

    /// <summary>
    /// Demonstrates basic allocation and freeing with NativeAllocator.
    /// </summary>
    private static void NativeAllocatorBasicExample()
    {
        var allocator = new NativeAllocator();

        // Allocate an array of 10 integers
        var slice = allocator.Allocate<int>(10);

        Console.WriteLine($"Allocated {slice.Length} integers");
        Console.WriteLine($"Pointer: {(IntPtr)slice.Pointer}");
        Console.WriteLine($"Byte length: {slice.ByteLength} bytes");

        // Write data to the allocated memory
        for (int i = 0; i < 10; i++)
        {
            slice.Pointer[i] = i * 42;
        }

        // Read data back
        Console.Write("Values: ");
        for (int i = 0; i < 10; i++)
        {
            Console.Write($"{slice.Pointer[i]} ");
        }
        Console.WriteLine();

        // Free the memory when done
        allocator.Free(slice.Pointer);
        Console.WriteLine("Memory freed");
    }

    /// <summary>
    /// Demonstrates reallocation with NativeAllocator.
    /// </summary>
    private static void NativeAllocatorReallocateExample()
    {
        var allocator = new NativeAllocator();

        // Initial allocation
        var slice = allocator.Allocate<int>(5);
        for (int i = 0; i < 5; i++)
        {
            slice.Pointer[i] = i * 10;
        }

        Console.WriteLine($"Original: length={slice.Length}, capacity=5");
        Console.Write("Values: ");
        for (int i = 0; i < 5; i++)
        {
            Console.Write($"{slice.Pointer[i]} ");
        }
        Console.WriteLine();

        // Reallocate to larger size (preserves existing data)
        var newSlice = allocator.Reallocate<int>(slice.Pointer, 10);

        Console.WriteLine($"After reallocate: length={newSlice.Length}, capacity=10");
        Console.Write("Values (first 5 preserved): ");
        for (int i = 0; i < 10; i++)
        {
            Console.Write($"{newSlice.Pointer[i]} ");
        }
        Console.WriteLine();

        // Clean up
        allocator.Free(newSlice.Pointer);
    }

    /// <summary>
    /// Demonstrates basic arena allocation.
    /// </summary>
    private static void ArenaAllocatorBasicExample()
    {
        // Create arena with 4KB initial capacity
        using var arena = ArenaAllocator.Create(4096);

        Console.WriteLine($"Arena capacity: {arena.CapacityBytes} bytes");

        // Fast allocations (bump pointer - no individual free needed)
        var ints = arena.Allocate<int>(100);
        var longs = arena.Allocate<long>(50);
        var bytes = arena.Allocate<byte>(200);

        Console.WriteLine($"Allocated: {ints.Length} ints, {longs.Length} longs, {bytes.Length} bytes");
        Console.WriteLine($"Total allocated: {arena.AllocatedBytes} bytes");

        // Use the memory
        for (int i = 0; i < 100; i++)
        {
            ints.Pointer[i] = i;
        }

        Console.WriteLine("Data written to arena memory");
    }

    /// <summary>
    /// Demonstrates arena reset to free all allocations at once.
    /// </summary>
    private static void ArenaAllocatorResetExample()
    {
        using var arena = ArenaAllocator.Create(1024);

        // Allocate some memory
        arena.Allocate<int>(100);
        arena.Allocate<long>(50);

        Console.WriteLine($"Before reset: {arena.AllocatedBytes} bytes allocated");

        // Reset - frees ALL allocations instantly (O(1))
        arena.Reset();

        Console.WriteLine($"After reset: {arena.AllocatedBytes} bytes allocated");
        Console.WriteLine($"Capacity unchanged: {arena.CapacityBytes} bytes");

        // Memory is now reusable
        var newSlice = arena.Allocate<int>(10);
        Console.WriteLine($"New allocation after reset: {newSlice.Length} ints");
    }

    /// <summary>
    /// Demonstrates automatic arena growth when capacity is exceeded.
    /// </summary>
    private static void ArenaAllocatorGrowthExample()
    {
        // Start with very small arena (64 bytes)
        using var arena = ArenaAllocator.Create(64);

        Console.WriteLine($"Initial capacity: {arena.CapacityBytes} bytes");

        // This allocation exceeds the initial capacity
        var largeSlice = arena.Allocate<int>(1000); // Needs 4000 bytes

        Console.WriteLine($"After large allocation: {arena.AllocatedBytes} bytes allocated");
        Console.WriteLine($"Arena grew to: {arena.CapacityBytes} bytes");
        Console.WriteLine($"Allocated slice length: {largeSlice.Length}");
    }

    /// <summary>
    /// Demonstrates working with MemorySlice.
    /// </summary>
    private static void MemorySliceExample()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        // Fill with data
        for (int i = 0; i < 10; i++)
        {
            slice.Pointer[i] = i * 2;
        }

        // Create a MemorySlice from the raw pointer and length
        var memorySlice = new MemorySlice<int>(slice.Pointer, slice.Length);

        Console.WriteLine($"MemorySlice length: {memorySlice.Length}");
        Console.WriteLine($"MemorySlice byte length: {memorySlice.ByteLength}");
        Console.WriteLine($"MemorySlice pointer: {(IntPtr)memorySlice.Pointer}");

        // Deconstruct
        memorySlice.Deconstruct(out int* ptr, out nuint len);
        Console.WriteLine($"Deconstructed: pointer={(IntPtr)ptr}, length={len}");

        // Use as IMemorySlice
        IMemorySlice<int, nuint> iSlice = memorySlice;
        Console.WriteLine($"As IMemorySlice: length={iSlice.Length}");

        allocator.Free(slice.Pointer);
    }

    /// <summary>
    /// Demonstrates LongSpan for type-safe memory access.
    /// </summary>
    private static void LongSpanExample()
    {
        var allocator = new NativeAllocator();
        var slice = allocator.Allocate<int>(10);

        // Initialize data
        for (int i = 0; i < 10; i++)
        {
            slice.Pointer[i] = i * 100;
        }

        // Create LongSpan over the memory
        var span = new LongSpan<int>(slice.Pointer, 10);

        Console.WriteLine($"LongSpan length: {span.Length}");
        Console.WriteLine($"Is empty: {span.IsEmpty}");

        // Read via indexer (bounds checked)
        Console.Write("Values: ");
        for (long i = 0; i < span.Length; i++)
        {
            Console.Write($"{span[i]} ");
        }
        Console.WriteLine();

        // Write via indexer
        span[0] = 999;
        span[5] = 555;
        Console.WriteLine($"After modification: span[0]={span[0]}, span[5]={span[5]}");

        // Bounds checking
        try
        {
            _ = span[10]; // Out of bounds
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("Bounds checking works: IndexOutOfRangeException thrown for index 10");
        }

        allocator.Free(slice.Pointer);
    }

    /// <summary>
    /// Demonstrates using allocators with multiple unmanaged types.
    /// </summary>
    private static void MultipleTypesExample()
    {
        using var arena = ArenaAllocator.Create(8192);

        // Allocate arrays of different types
        var ints = arena.Allocate<int>(100);
        var longs = arena.Allocate<long>(100);
        var floats = arena.Allocate<float>(100);
        var doubles = arena.Allocate<double>(100);
        var bytes = arena.Allocate<byte>(1000);

        Console.WriteLine($"Allocated:");
        Console.WriteLine($"  int[100]    - {ints.ByteLength} bytes");
        Console.WriteLine($"  long[100]   - {longs.ByteLength} bytes");
        Console.WriteLine($"  float[100]  - {floats.ByteLength} bytes");
        Console.WriteLine($"  double[100] - {doubles.ByteLength} bytes");
        Console.WriteLine($"  byte[1000]  - {bytes.ByteLength} bytes");
        Console.WriteLine($"  Total: {arena.AllocatedBytes} bytes");

        // Use the data
        for (int i = 0; i < 100; i++)
        {
            ints.Pointer[i] = i;
            longs.Pointer[i] = i * 2L;
            floats.Pointer[i] = i * 0.5f;
            doubles.Pointer[i] = i * 0.25;
        }

        // Verify
        Console.WriteLine($"Verification: ints[50]={ints.Pointer[50]}, longs[50]={longs.Pointer[50]}, floats[50]={floats.Pointer[50]:F1}, doubles[50]={doubles.Pointer[50]:F2}");
    }
}