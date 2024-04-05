namespace VinZ.Common;

public record ProgramInfo(string? ExeName, string ExeVersion, string BuildConfiguration, bool IsDebug)
{
    public void Print()
    {
        Console.WriteLine(@$"
{ExeName} {ExeVersion} ({BuildConfiguration})
");
    }


    public static ProgramInfo Current 
        
        => GetProgramInfo(Assembly.GetEntryAssembly());


    public static ProgramInfo GetProgramInfo(Assembly assembly)
    {
        var dllPath = assembly.Location;
        var rx = new Regex(@"^(?<path>.*)\.dll$");
        var match = rx.Match(dllPath);
        var exePath = !match.Success ? default : match.Result("${path}.exe");
        var exeName = exePath == default ? default : Path.GetFileName(exePath);
        var exeVersion = exePath == default ? "???" : System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath).FileVersion;

        var isDebug = IsDebugBuild(assembly);
        var buildConfiguration = isDebug ? "Debug" : "Release";

        return new ProgramInfo(exeName, exeVersion, buildConfiguration, isDebug);
    }

    public static bool IsDebugBuild(Assembly assembly)
    {
        return assembly
            .GetCustomAttributes(false)
            .OfType<DebuggableAttribute>()
            .Any(attr => attr.IsJITTrackingEnabled);
    }
}