using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnrealSharp.GlueGenerator.NativeTypes;

namespace UnrealSharp.GlueGenerator;

public interface ITypeDiscoveryResult
{
    string Name { get; }
}

public sealed class TypeDiscoveryResult(UnrealType type)
    : ITypeDiscoveryResult
{
    public UnrealType Type { get; } = type;

    public string Name => Type.SourceName;
}

public sealed class TypeDiscoveryError(Exception exception) : ITypeDiscoveryResult
{
    public string Name => "???";
    
    public Exception Exception { get; } = exception;
}