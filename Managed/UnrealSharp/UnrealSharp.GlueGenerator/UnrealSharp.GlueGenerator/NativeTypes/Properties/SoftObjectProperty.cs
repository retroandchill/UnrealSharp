using Microsoft.CodeAnalysis;

namespace UnrealSharp.GlueGenerator.NativeTypes.Properties;

public record SoftObjectProperty : TemplateProperty
{
    public SoftObjectProperty(SyntaxNode syntaxNode, ISymbol memberSymbol, ITypeSymbol? typeSymbol, UnrealType outer) 
        : base(syntaxNode, memberSymbol, typeSymbol, PropertyType.SoftObject, outer, "SoftObjectMarshaller")
    {
        
    }
    
    public SoftObjectProperty(ObjectProperty innerProperty, string sourceName, Accessibility accessibility, string protection, UnrealType outer) 
        : base(new EquatableArray<UnrealProperty>([innerProperty]), PropertyType.SoftObject, 
            $"SoftObjectPtr<{innerProperty.ManagedType}", "SoftObjectMarshaller", sourceName, 
            accessibility, protection, outer)
    {
        CacheNativeTypePtr = true;
    }
}