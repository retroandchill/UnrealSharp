using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace UnrealSharp.GlueGenerator.NativeTypes.Properties;

public record SimpleProperty : UnrealProperty
{
    protected string ShortEngineName;
    
    public SimpleProperty(SyntaxNode syntaxNode, ISymbol memberSymbol, ITypeSymbol? typeSymbol, PropertyType propertyType, UnrealType outer) 
        : base(syntaxNode, memberSymbol, typeSymbol, propertyType, outer)
    {
        ManagedType = $"global::{Namespace}.{typeSymbol!.Name}";
        ShortEngineName = typeSymbol.Name.Substring(1);
    }

    public SimpleProperty(PropertyType type, string managedType, string sourceName, Accessibility accessibility, UnrealType outer) : base(type, sourceName, accessibility, outer)
    {
        ManagedType = managedType;
        ShortEngineName = managedType.Split('.').Last().Substring(1);
    }
}