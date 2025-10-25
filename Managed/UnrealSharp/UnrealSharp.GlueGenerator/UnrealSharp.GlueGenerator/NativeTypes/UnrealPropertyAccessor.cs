using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnrealSharp.GlueGenerator.NativeTypes;

public record UnrealPropertyAccessor : UnrealFunction
{
    public readonly string PropertyName;
    public readonly bool IsSetter;
    
    public UnrealPropertyAccessor(SemanticModel model, ISymbol typeSymbol, PropertyDeclarationSyntax syntax, 
                                  bool isSetter, UnrealType outer) : base(model, typeSymbol, syntax, isSetter, outer)
    {
        PropertyName = syntax.Identifier.Text;
        IsSetter = isSetter;
    }

    protected override void ExportFunctionCallString(GeneratorStringBuilder builder)
    {
        builder.Append(IsSetter
            ? $"{PropertyName} = {Properties.Select(p => p.RefKind.RefKindToString() + p.SourceName).Single()};"
            : $"{PropertyName};");
    }
}