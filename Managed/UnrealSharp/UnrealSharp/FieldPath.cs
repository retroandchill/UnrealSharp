using System.Runtime.InteropServices;
using UnrealSharp.Core;
using UnrealSharp.Core.Marshallers;
using UnrealSharp.CoreUObject;

namespace UnrealSharp;

[StructLayout(LayoutKind.Sequential)]
public struct FFieldPathUnsafe {
    internal IntPtr ResolvedField;
#if !PACKAGE
    internal IntPtr InitialFieldClass;
    internal int FieldPathSerialNumber;
#endif
    internal TWeakObjectPtr<UStruct> ResolvedOwner;
    internal UnmanagedArray Path;

}

[StructLayout(LayoutKind.Sequential)]
public struct FFieldPath {
    internal FFieldPathUnsafe PathUnsafe;

    public FFieldPath(FFieldPathUnsafe pathUnsafe) {
        PathUnsafe = pathUnsafe;
    }
}

public static class FieldPathMarshaller
{
    public static void ToNative(IntPtr nativeBuffer, int arrayIndex, FFieldPath obj)
    {
        BlittableMarshaller<FFieldPathUnsafe>.ToNative(nativeBuffer, arrayIndex, obj.PathUnsafe);
    }
    
    public static FFieldPath FromNative(IntPtr nativeBuffer, int arrayIndex)
    {
        FFieldPathUnsafe fieldPathUnsafe = BlittableMarshaller<FFieldPathUnsafe>.FromNative(nativeBuffer, arrayIndex);
        return new FFieldPath(fieldPathUnsafe);
    }
}