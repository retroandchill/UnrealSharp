namespace UnrealSharp.Core;

public static class StartUpJobManager
{
    private static readonly Dictionary<string, List<Action>> AssemblyStartupJobs = new();
    private static readonly Dictionary<string, List<Action<IntPtr>>> TypeAdditionalBuildJobs = new();
    
    public static void RegisterStartUpJob(string assemblyName, Action initializer)
    {
        if (!AssemblyStartupJobs.TryGetValue(assemblyName, out List<Action>? value))
        {
            value = new List<Action>();
            AssemblyStartupJobs[assemblyName] = value;
        }

        value.Add(initializer);
    }

    public static void RegisterTypeAdditionalBuildJob(string typeName, Action<IntPtr> initializer)
    {
        if (!TypeAdditionalBuildJobs.TryGetValue(typeName, out List<Action<IntPtr>>? value))
        {
            value = new List<Action<IntPtr>>();
            TypeAdditionalBuildJobs[typeName] = value;
        }
        
        value.Add(initializer);
    }
    
    public static void RunStartUpJobForAssembly(string assemblyName)
    {
        if (!AssemblyStartupJobs.TryGetValue(assemblyName, out List<Action>? initializers))
        {
            return;
        }
        
        foreach (Action initializer in initializers)
        {
            initializer();
        }
            
        AssemblyStartupJobs.Remove(assemblyName);
    }

    public static void RunTypeAdditionalBuildJob(string typeName, IntPtr nativePtr)
    {
        if (!TypeAdditionalBuildJobs.TryGetValue(typeName, out List<Action<IntPtr>>? initializers))
        {
            return;
        }
        
        foreach (Action<IntPtr> initializer in initializers)
        {
            initializer(nativePtr);
        }
        
        TypeAdditionalBuildJobs.Remove(typeName);   
    }
    
    public static bool HasJobsForAssembly(string assemblyName) => AssemblyStartupJobs.ContainsKey(assemblyName);
}