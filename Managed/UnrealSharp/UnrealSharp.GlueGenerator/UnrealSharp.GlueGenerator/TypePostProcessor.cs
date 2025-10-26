using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnrealSharp.GlueGenerator.NativeTypes;

namespace UnrealSharp.GlueGenerator;

public interface ITypePostProcessor
{
    int Priority { get; }
    
    bool CanProcess(UnrealType type);
    
    void Process(UnrealType type, MemberDeclarationSyntax syntax, INamedTypeSymbol typeSymbol, SourceProductionContext context);
}