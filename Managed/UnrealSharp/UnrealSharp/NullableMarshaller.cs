using UnrealSharp.Core;
using UnrealSharp.Core.Marshallers;
using UnrealSharp.Interop;

namespace UnrealSharp;

public class NullableMarshaller<T>(IntPtr nativeProperty, MarshallingDelegates<T>.ToNative toNative, MarshallingDelegates<T>.FromNative fromNative) where T : struct
{
    public void ToNative(IntPtr nativeBuffer, int arrayIndex, T? obj)
    {
        if (obj.HasValue)
        {
            var result = FOptionalPropertyExporter.CallMarkSetAndGetInitializedValuePointerToReplace(nativeProperty, nativeBuffer);
            toNative(result, 0, obj.Value);
        }
        else
        {
            FOptionalPropertyExporter.CallMarkUnset(nativeProperty, nativeBuffer);
        }
    }

    public T? FromNative(IntPtr nativeBuffer, int arrayIndex)
    {
        if (!FOptionalPropertyExporter.CallIsSet(nativeProperty, nativeBuffer).ToManagedBool()) return null;
        var result = FOptionalPropertyExporter.CallGetValuePointerForRead(nativeProperty, nativeBuffer);
        return fromNative(result, 0);
    }
    
    public void DestructInstance(IntPtr nativeBuffer, int arrayIndex)
    {
        FOptionalPropertyExporter.CallDestructInstance(nativeProperty, nativeBuffer);
    }
}