using Mono.Cecil;
using UnrealSharpWeaver.MetaData;
using UnrealSharpWeaver.Utilities;

namespace UnrealSharpWeaver.NativeTypes;

internal class NativeDataNullableType(TypeReference propertyTypeRef, TypeReference innerTypeReference, int arrayDim)
    : NativeDataContainerType(propertyTypeRef, arrayDim, PropertyType.Optional, innerTypeReference)
    {
        
        protected override AssemblyDefinition MarshallerAssembly => WeaverImporter.Instance.UnrealSharpCoreAssembly;
        protected override string MarshallerNamespace => WeaverImporter.UnrealSharpCoreMarshallers;
        
        protected override bool AllowsSetter => true;
        
    public override string GetContainerMarshallerName()
    {
        return "NullableMarshaller`1";
    }

    public override string GetCopyContainerMarshallerName()
    {
        return "NullableMarshaller`1";
    }
}