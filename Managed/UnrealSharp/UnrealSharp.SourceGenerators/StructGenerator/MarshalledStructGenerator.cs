using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnrealSharp.SourceGenerators.Model;

namespace UnrealSharp.SourceGenerators.StructGenerator;

[Generator]
public class MarshalledStructGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider((n, _) => n is StructDeclarationSyntax or RecordDeclarationSyntax,
                (ctx, _) =>
                {
                    var structDeclaration = ctx.Node;
                    if (ctx.SemanticModel.GetDeclaredSymbol(structDeclaration) is not INamedTypeSymbol { TypeKind: TypeKind.Struct } structSymbol)
                    {
                        return null;
                    }

                    return AnalyzerStatics.HasAttribute(structSymbol, AnalyzerStatics.UStructAttribute)
                           && !AnalyzerStatics.HasAttribute(structSymbol, AnalyzerStatics.GeneratedTypeAttribute)
                           && structSymbol.ToDisplayString() != "UnrealSharp.FName"
                        ? structSymbol
                        : null;
                })
            .Where(sym => sym is not null);

        context.RegisterSourceOutput(syntaxProvider, GenerateStruct!);
    }

    private static void GenerateStruct(SourceProductionContext context, INamedTypeSymbol structSymbol)
    {
        using var builder = new SourceBuilder();

        WriteStructCode(builder, structSymbol);

        context.AddSource($"{structSymbol.Name}.generated.cs", builder.ToString());
    }

    private static void WriteStructCode(SourceBuilder builder, INamedTypeSymbol structSymbol)
    {
        builder.AppendLine("using UnrealSharp;");
        builder.AppendLine("using UnrealSharp.Core.Attributes;");
        builder.AppendLine("using UnrealSharp.Core.Marshallers;");
        builder.AppendLine("using UnrealSharp.Interop;");
        builder.AppendLine();
        builder.AppendLine($"namespace {structSymbol.ContainingNamespace.ToDisplayString()};");
        builder.AppendLine();

        var engineName = structSymbol.GetNameWithoutPrefix();
        var namespaceName = structSymbol.ContainingNamespace;
        var fullName = $"{namespaceName}.{engineName}";

        builder.AppendLine($"[GeneratedType(\"{engineName}\", \"{fullName}\")]");
        builder.AppendLine(
            $"partial {(structSymbol.IsRecord ? "record " : "")}struct {structSymbol.Name} : MarshalledStruct<{structSymbol.Name}>");

        using var structScope = builder.OpenBlock();

        builder.AppendLine("public static readonly IntPtr NativeClassPtr;");
        builder.AppendLine("public static IntPtr GetNativeClassPtr() => NativeClassPtr;");

        var assemblyName = structSymbol.ContainingAssembly.Name;
        var nativeStructFromName =
            $"UCoreUObjectExporter.CallGetNativeStructFromName(\"{assemblyName}\", \"{engineName}\", \"{fullName}\")";
        builder.AppendLine(
            $"public static readonly int NativeDataSize = UScriptStructExporter.CallGetNativeStructSize({nativeStructFromName});");
        builder.AppendLine("public static int GetNativeDataSize() => NativeDataSize;");
        builder.AppendLine();

        var properties = GetProperties(structSymbol);
        foreach (var prop in properties)
        {
            builder.AppendLine($"public static readonly int {prop.Name}_Offset;");
            if (!prop.MarshallerInstanced) continue;
            
            builder.AppendLine($"private static readonly IntPtr {prop.Name}_NativeProperty;");
            builder.AppendLine($"private static {prop.MarshallerInfo.Name}? {prop.Name}_Marshaller = null;");
            builder.AppendLine();
        }

        builder.AppendLine($"static {structSymbol.Name}()");
        using (builder.OpenBlock())
        {
            builder.AppendLine(
                $"NativeClassPtr = UCoreUObjectExporter.CallGetNativeStructFromName(typeof({structSymbol.Name}).GetAssemblyName(), \"{namespaceName}\", \"{engineName}\");");
            foreach (var property in properties)
            {
                var prefix = !property.MarshallerInstanced ? "IntPtr " : "";
                builder.AppendLine(
                    $"{prefix}{property.Name}_NativeProperty = FPropertyExporter.CallGetNativePropertyFromName(NativeClassPtr, \"{property.Name}\");");
                builder.AppendLine(
                    $"{property.Name}_Offset = FPropertyExporter.CallGetPropertyOffset({property.Name}_NativeProperty);");
            }
            builder.AppendLine("NativeDataSize = UScriptStructExporter.CallGetNativeStructSize(NativeClassPtr);");
        }
        builder.AppendLine();
        
        builder.AppendLine($"public {structSymbol.Name}(IntPtr InNativeStruct)");
        using (builder.OpenBlock())
        {
            builder.AppendLine("unsafe");
            using var unsafeBlock = builder.OpenBlock();
            foreach (var property in properties)
            {
                GenerateMarshallerInitialization(builder, property);
                builder.AppendLine(property.MarshallerInstanced
                    ? $"{property.Name} = {property.Name}_Marshaller.FromNative({property.Name}_NativeBuffer, 0);"
                    : $"{property.Name} = {property.MarshallerInfo.Name}.FromNative({property.Name}_NativeBuffer, 0);");
            }
        }
        builder.AppendLine();
        
        builder.AppendLine($"public static {structSymbol.Name} FromNative(IntPtr InNativeStruct) => new {structSymbol.Name}(InNativeStruct);");
        builder.AppendLine();

        builder.AppendLine("public void ToNative(IntPtr buffer)");
        using (builder.OpenBlock())
        {
            builder.AppendLine("unsafe");
            using var unsafeBlock = builder.OpenBlock();
            foreach (var property in properties)
            {
                GenerateMarshallerInitialization(builder, property, "buffer");
                builder.AppendLine(property.MarshallerInstanced
                    ? $"{property.Name}_Marshaller.ToNative({property.Name}_NativeBuffer, 0, {property.Name});"
                    : $"{property.MarshallerInfo.Name}.ToNative({property.Name}_NativeBuffer, 0, {property.Name});");
            }
        }
    }

    private static void GenerateMarshallerInitialization(SourceBuilder builder, AccessiblePropertyInfo property, string parameterName = "InNativeStruct")
    {
        if (property.MarshallerInstanced)
        {
            builder.Append($"{property.Name}_Marshaller ??= new {property.MarshallerInfo.Name}({property.Name}_NativeProperty, {property.MarshallerInfo.ChildMarshallerType}.ToNative, {property.MarshallerInfo.ChildMarshallerType}.FromNative");
            if (property.MarshallerHasValue)
            {
                builder.Append(
                    $", {property.MarshallerInfo.ValueMarshallerType}.ToNative, {property.MarshallerInfo.ValueMarshallerType}.FromNative");
            }
            builder.AppendLine(");");
        }

        builder.AppendLine($"IntPtr {property.Name}_NativeBuffer = IntPtr.Add({parameterName}, {property.Name}_Offset);");
    }

    public static ImmutableArray<AccessiblePropertyInfo> GetProperties(INamedTypeSymbol structSymbol)
    {
        return [
            ..structSymbol.GetMembers()
                .Where(m => m is IPropertySymbol or IFieldSymbol &&
                            AnalyzerStatics.HasAttribute(m, AnalyzerStatics.UPropertyAttribute))
                .Select(p => p.GetPropertyInfo())
        ];
    }
}

    