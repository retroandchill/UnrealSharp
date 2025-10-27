using Microsoft.CodeAnalysis;

namespace UnrealSharp.GlueGenerator.NativeTypes.Properties;

public record ArrayProperty : ContainerProperty
{
    public ArrayProperty(SyntaxNode syntaxNode, ISymbol memberSymbol, ITypeSymbol typeSymbol, UnrealType outer) 
        : base(syntaxNode, memberSymbol, typeSymbol, PropertyType.Array, outer)
    {
        
    }
    
    public ArrayProperty(UnrealProperty innerType, string sourceName, string managedType, Accessibility accessibility, string protection, UnrealType outer) 
        : base(new EquatableArray<UnrealProperty>([innerType]), PropertyType.Array, managedType, "", sourceName, accessibility, protection, outer)
    {
        
    }
    
    protected override string GetFieldMarshaller() => "ArrayMarshaller";
    protected override string GetCopyMarshaller() => "ArrayCopyMarshaller";
}