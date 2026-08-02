using System.Runtime.InteropServices;

namespace SharpAllocators;

public unsafe interface IAllocator
{
    public T* Allocate<T>() where T : unmanaged;
    public void Free<T>(T* pointer) where T : unmanaged;
    public T* Realloc<T>(T* pointer, long size) where T : unmanaged;
}
