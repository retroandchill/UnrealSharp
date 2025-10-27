using System;
using System.Collections.Generic;
using System.Linq;
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
                (ctx, _) =>
                {
                    try
                    {
                        UnrealType type = globalType.InspectAttributeDelegate!(null, ctx,
                            (MemberDeclarationSyntax)ctx.TargetNode,
                            ctx.Attributes)!;
                        return new TypeDiscoveryResult(type);
                    }
                    catch (Exception e)
                    {
                        return new TypeDiscoveryError(e);
                    }
                });

            var combined = newTypes.Combine(context.AnalyzerConfigOptionsProvider);
            
            context.RegisterSourceOutput(combined, static (spc, result) => EmitType(spc, result.Left));
        }
    }
    
    private static void EmitType(SourceProductionContext spc, ITypeDiscoveryResult result)
    {
        try
        {
            UnrealType utype = result switch
            {
                TypeDiscoveryResult r => r.Type,
                TypeDiscoveryError e => throw e.Exception,
                _ => throw new Exception("Unknown result type")
            };
            
            GeneratorStringBuilder builder = new GeneratorStringBuilder();
            builder.BeginGeneratedSourceFile(utype);
            
            builder.AppendLine();
            utype.ExportType(builder, spc);
            builder.BeginModuleInitializer(utype);
            
            spc.AddSource(utype.SourceName + ".g.cs", builder.ToString());
        }
        catch (Exception exception)
        {
            DiagnosticDescriptor descriptor = new DiagnosticDescriptor("UTDG001", "UnrealTypeDiscoveryGenerator Error", $"Error processing result '{result.Name}': {exception}", "UnrealTypeDiscoveryGenerator", DiagnosticSeverity.Error, true);
            spc.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
        }
    }
}