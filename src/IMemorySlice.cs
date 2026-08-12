namespace SharpAllocators;

public unsafe interface IMemorySlice<TPointer, TLength>
    where TPointer : unmanaged
    where TLength  : unmanaged
{
    public TPointer* Pointer { get; }
    public TLength   Length { get; }
}
