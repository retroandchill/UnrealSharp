using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnrealSharp.GlueGenerator.NativeTypes;

namespace UnrealSharp.GlueGenerator;

public interface ITypeDiscoveryResult;

public sealed class TypeDiscoveryResult(UnrealType type, MemberDeclarationSyntax syntax, INamedTypeSymbol typeSymbol)
    : ITypeDiscoveryResult
{
    public UnrealType Type { get; } = type;
    public MemberDeclarationSyntax Syntax { get; } = syntax;
    public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;
}

public sealed class TypeDiscoveryError(Exception exception) : ITypeDiscoveryResult
{
    public Exception Exception { get; } = exception;
}