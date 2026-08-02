using System.Runtime.InteropServices;

namespace SharpAllocators
{
    public readonly unsafe struct NativeAllocator : IAllocator
    {
        public T* Allocate<T>(long count) where T : unmanaged
        {
            return (T*)NativeMemory.Alloc((nuint)count);
        }

        public void Free<T>(T* pointer) where T : unmanaged
        {
            NativeMemory.Free(pointer);
        }

        public T* Reallocate<T>(T* pointer, long count) where T : unmanaged
        {
            return (T*)NativeMemory.Realloc(pointer, (nuint)count);
        }
    }
}
