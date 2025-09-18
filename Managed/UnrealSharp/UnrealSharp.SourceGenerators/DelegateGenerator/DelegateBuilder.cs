using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace UnrealSharp.SourceGenerators.DelegateGenerator;

public abstract class DelegateBuilder
{
    public abstract void StartBuilding(StringBuilder stringBuilder, INamedTypeSymbol delegateSymbol, string className, bool generateInvoker);
    
    protected void GenerateGetInvoker(StringBuilder stringBuilder, INamedTypeSymbol delegateSymbol)
    {
        stringBuilder.AppendLine($"    protected override {delegateSymbol} GetInvoker()");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        return Invoker;");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine();
    }
    
    protected void GenerateInvoke(StringBuilder stringBuilder, INamedTypeSymbol delegateSymbol)
    {
        if (delegateSymbol.DelegateInvokeMethod == null)
        {
            return;
        }
        
        var returnType = delegateSymbol.DelegateInvokeMethod.ReturnsVoid ? "void" :delegateSymbol.DelegateInvokeMethod.ReturnType.ToDisplayString();
        
        if (delegateSymbol.DelegateInvokeMethod.Parameters.IsEmpty)
        {
            stringBuilder.AppendLine($"    protected {returnType} Invoker()");
        }
        else
        {
            stringBuilder.Append($"    protected {returnType} Invoker(");
            stringBuilder.Append(string.Join(", ", delegateSymbol.DelegateInvokeMethod.Parameters.Select(x => $"{DelegateWrapperGenerator.GetRefKindKeyword(x)}{x.Type} {x.Name}")));
            stringBuilder.Append(")");
            stringBuilder.AppendLine();
        }
        
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        ProcessDelegate(IntPtr.Zero);");
        if (!delegateSymbol.DelegateInvokeMethod.ReturnsVoid)
        {
            stringBuilder.AppendLine("        return default!;");
        }
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine();
    }
}