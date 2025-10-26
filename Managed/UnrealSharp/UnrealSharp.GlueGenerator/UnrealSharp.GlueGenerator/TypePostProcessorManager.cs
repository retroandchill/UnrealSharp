using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnrealSharp.GlueGenerator.NativeTypes;

namespace UnrealSharp.GlueGenerator;

[AttributeUsage(AttributeTargets.Class)]
public class TypePostProcessorProviderAttribute : Attribute;

public static class TypePostProcessorManager
{
    private static readonly List<ITypePostProcessor> _processors = [];
    private static readonly HashSet<Assembly> _loadedAssemblies = [];
    private static bool _initialized;
    
    public static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        
        // Load from current assembly
        LoadPostProcessorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Discover and load from other assemblies in the current domain
        LoadPostProcessorsFromDiscoveredAssemblies();
    }
    
    public static void LoadPostProcessorsFromAssembly(Assembly assembly)
    {
        if (!_loadedAssemblies.Add(assembly))
            return; // Already loaded

        try
        {
            foreach (Type type in GetTypesFromAssembly(assembly))
            {
                // Look for types marked with our provider attribute
                if (!type.IsDefined(typeof(TypePostProcessorProviderAttribute), inherit: false))
                    continue;

                // Find static methods that return ITypePostProcessor
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (typeof(ITypePostProcessor).IsAssignableFrom(method.ReturnType) && 
                        method.GetParameters().Length == 0)
                    {
                        try
                        {
                            if (method.Invoke(null, null) is ITypePostProcessor processor)
                            {
                                RegisterPostProcessor(processor);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log or handle creation errors
                            System.Diagnostics.Debug.WriteLine($"Failed to create post-processor from {method.Name}: {ex.Message}");
                        }
                    }
                }

                // Also look for types that directly implement ITypePostProcessor
                if (typeof(ITypePostProcessor).IsAssignableFrom(type) && 
                    !type.IsInterface && !type.IsAbstract)
                {
                    try
                    {
                        if (Activator.CreateInstance(type) is ITypePostProcessor processor)
                        {
                            RegisterPostProcessor(processor);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log or handle creation errors
                        System.Diagnostics.Debug.WriteLine($"Failed to create post-processor {type.Name}: {ex.Message}");
                    }
                }
            }
        }
        catch (ReflectionTypeLoadException)
        {
            // Skip assemblies that can't be fully loaded
        }
    }
    
    public static void LoadPostProcessorsFromDiscoveredAssemblies()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            // Skip already loaded assemblies
            if (_loadedAssemblies.Contains(assembly))
                continue;

            // Skip system assemblies to improve performance
            if (IsSystemAssembly(assembly))
                continue;

            LoadPostProcessorsFromAssembly(assembly);
        }
    }
    
    public static void RegisterPostProcessor(ITypePostProcessor processor)
    {
        _processors.Add(processor);
        // Sort by priority (highest first)
        _processors.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    /// <summary>
    /// Process a type through all registered post-processors
    /// </summary>
    public static void ProcessType(UnrealType type, MemberDeclarationSyntax syntax, INamedTypeSymbol typeSymbol, SourceProductionContext context)
    {
        Initialize();

        foreach (var processor in _processors.Where(processor => processor.CanProcess(type)))
        {
            try
            {
                processor.Process(type, syntax, typeSymbol, context);
            }
            catch (Exception ex)
            {
                // Report diagnostic instead of crashing the generator
                var descriptor = new DiagnosticDescriptor(
                    "UTDG002", 
                    "Type Post-Processor Error", 
                    $"Post-processor {processor.GetType().Name} failed: {ex.Message}", 
                    "UnrealTypeDiscoveryGenerator", 
                    DiagnosticSeverity.Warning, 
                    true);
                    
                context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
            }
        }
    }

    private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Return only the types that loaded successfully
            return ex.Types.Where(t => t != null)!;
        }
    }

    private static bool IsSystemAssembly(Assembly assembly)
    {
        string assemblyName = assembly.FullName ?? "";
        return assemblyName.StartsWith("System.") ||
               assemblyName.StartsWith("Microsoft.") ||
               assemblyName.StartsWith("mscorlib") ||
               assemblyName.StartsWith("netstandard");
    }
}