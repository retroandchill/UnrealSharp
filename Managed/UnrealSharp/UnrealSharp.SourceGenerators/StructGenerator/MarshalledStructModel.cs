using Microsoft.CodeAnalysis;

namespace UnrealSharp.SourceGenerators.StructGenerator;

public record MarshalledStructModel(INamedTypeSymbol StructSymbol)
{
    public string StructName => StructSymbol.Name;
}