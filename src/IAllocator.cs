namespace SharpAllocators;

public unsafe interface IAllocator
{
    public T* Allocate<T>(long count) where T : unmanaged;
    public void Free<T>(T* pointer) where T : unmanaged;
    public T* Realloc<T>(T* pointer, long count) where T : unmanaged;
}
