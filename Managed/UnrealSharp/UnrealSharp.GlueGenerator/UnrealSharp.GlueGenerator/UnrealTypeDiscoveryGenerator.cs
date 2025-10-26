using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnrealSharp.GlueGenerator.NativeTypes;

namespace UnrealSharp.GlueGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class UnrealTypeDiscoveryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        List<InspectorData> inspectors = InspectorManager.GetScopedInspectorData("Global");
        SyntaxValueProvider provider = context.SyntaxProvider;
        
        foreach (InspectorData globalType in inspectors)
        {
            IncrementalValuesProvider<ITypeDiscoveryResult> newTypes = provider.ForAttributeWithMetadataName<ITypeDiscoveryResult>(
                globalType.InspectAttribute.FullyQualifiedAttributeName, static (_, _) => true,
                static (ctx, _) =>
                {
                    try
                    {
                        InspectorData decode =
                            InspectorManager.GetInspectorData(ctx.Attributes[0].AttributeClass!.Name)!;
                        UnrealType type = decode.InspectAttributeDelegate!(null, ctx,
                            (MemberDeclarationSyntax)ctx.TargetNode,
                            ctx.Attributes)!;
                        return new TypeDiscoveryResult(type, (MemberDeclarationSyntax)ctx.TargetNode, 
                            (INamedTypeSymbol) ctx.SemanticModel.GetSymbolInfo(ctx.TargetNode).Symbol!);
                    }
                    catch (Exception e)
                    {
                        return new TypeDiscoveryError(e);
                    }
                });
            
            context.RegisterSourceOutput(newTypes, static (spc, result) => EmitType(spc, result));
        }
    }
    
    private static void EmitType(SourceProductionContext spc, ITypeDiscoveryResult result)
    {
        try
        {
            TypeDiscoveryResult validResult = result switch
            {
                TypeDiscoveryResult r => r,
                TypeDiscoveryError e => throw e.Exception,
                _ => throw new Exception("Unknown result type")
            };
            
            UnrealType utype = validResult.Type;
            TypePostProcessorManager.ProcessType(utype, validResult.Syntax, validResult.TypeSymbol, spc);
            
            GeneratorStringBuilder builder = new GeneratorStringBuilder();
            builder.BeginGeneratedSourceFile(utype);
            
            builder.AppendLine();
            utype.ExportType(builder, spc);
            builder.BeginModuleInitializer(utype);
            
            spc.AddSource(utype.SourceName + ".g.cs", builder.ToString());
        }
        catch (Exception exception)
        {
            DiagnosticDescriptor descriptor = new DiagnosticDescriptor("UTDG001", "UnrealTypeDiscoveryGenerator Error", exception.ToString(), "UnrealTypeDiscoveryGenerator", DiagnosticSeverity.Error, true);
            spc.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
        }
    }
}