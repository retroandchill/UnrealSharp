using Microsoft.CodeAnalysis;

namespace UnrealSharp.GlueGenerator.NativeTypes.Properties;

public record NameProperty : BlittableProperty
{
    public NameProperty(SyntaxNode syntaxNode, ISymbol memberSymbol, ITypeSymbol? typeSymbol, UnrealType outer) : base(syntaxNode, memberSymbol, typeSymbol, PropertyType.Name, outer)
    {
    }
    
    public NameProperty(string sourceName, Accessibility accessibility, UnrealType outer) 
        : base(PropertyType.Name, "UnrealSharp.Core.FName", sourceName, accessibility, outer)
    {
    }
}