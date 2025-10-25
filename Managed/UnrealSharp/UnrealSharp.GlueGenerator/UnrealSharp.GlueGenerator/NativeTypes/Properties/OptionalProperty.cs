using System.Linq;
using Microsoft.CodeAnalysis;

namespace UnrealSharp.GlueGenerator.NativeTypes.Properties;

public record OptionalProperty : ContainerProperty
{
    const string OptionalMarshaller = "OptionalMarshaller";
    
    public OptionalProperty(SyntaxNode syntaxNode, ISymbol memberSymbol, ITypeSymbol typeSymbol, UnrealType outer) 
        : base(syntaxNode, memberSymbol, typeSymbol, PropertyType.Optional, outer)
    {

    }
    
    public override void ExportToNative(GeneratorStringBuilder builder, string buffer, string value)
    {
        string delegates = string.Join(", ", InnerTypes.Select(t => t).Select(t => $"{t.CallToNative}, {t.CallFromNative}"));
        builder.AppendLine($"{InstancedMarshallerVariable} ??= new {MarshallerType}({NativePropertyVariable}, {delegates});");
        builder.AppendLine();
        
        AppendCallToNative(builder, InstancedMarshallerVariable, buffer, value);
    }

    protected override string GetFieldMarshaller() => OptionalMarshaller;
    protected override string GetCopyMarshaller() => OptionalMarshaller;
}