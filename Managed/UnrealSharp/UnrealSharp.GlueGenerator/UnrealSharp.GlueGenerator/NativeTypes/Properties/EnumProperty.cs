using Microsoft.CodeAnalysis;

namespace UnrealSharp.GlueGenerator.NativeTypes.Properties;

public record EnumProperty : FieldProperty
{
    public override string MarshallerType => $"EnumMarshaller<{ManagedType}>";

    public EnumProperty(SyntaxNode syntaxNode, ISymbol memberSymbol, ITypeSymbol? typeSymbol, UnrealType outer) 
        : base(syntaxNode, memberSymbol, typeSymbol, PropertyType.Enum, outer)
    {
        
    }
    
    public EnumProperty(string managedType, string sourceName, Accessibility accessibility, UnrealType outer) 
        : base(PropertyType.Enum, managedType, sourceName, accessibility, outer)
    {
        
    }
}