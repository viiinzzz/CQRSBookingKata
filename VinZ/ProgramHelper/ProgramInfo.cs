/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace VinZ.Common;

public class ProgramInfo
{
    private ProgramInfo() {}

    private static readonly ProgramInfo Instance = new();

    public static ProgramInfo Get() => Instance;


    public string ProgName { get; } = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location ?? string.Empty).Trim();
    
    public string ExeName =
        Process.GetCurrentProcess().MainModule?.FileName == default ? string.Empty
            : Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule!.FileName) ?? string.Empty;

    public string ExeVersion = 
        Process.GetCurrentProcess().MainModule?.FileName == default ? string.Empty
            : FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule!.FileName).FileVersion?.Trim() ?? string.Empty;


    public string Env { get; } = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "undefined";

    public bool IsDebug { get; } = IsDebugBuild(Assembly.GetExecutingAssembly());

    public bool IsRelease => !IsDebugBuild(Assembly.GetExecutingAssembly());

    public string BuildConfiguration { get; } = IsDebugBuild(Assembly.GetExecutingAssembly()) ? "Debug" : "Release";


    public string ProcessorArchitecture { get; } = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")?.ToLower() ?? string.Empty;

    public string ProcessArchitecture { get; } = RuntimeInformation.ProcessArchitecture.ToString().ToLower();

    public string Os { get; } = RuntimeInformation.OSDescription;

    public string Framework { get; } = RuntimeInformation.FrameworkDescription;


    public override string ToString() => $"{ExeName} v{ExeVersion} Build {BuildConfiguration} {ProcessArchitecture} ({Env})";



    private static readonly string[] True = ["1", "true"];

    public bool IsTrueEnv(string variableName)
    {
        return True.Contains(Environment.GetEnvironmentVariable(variableName)?.ToLower() ?? string.Empty);
    }


    private static bool IsDebugBuild(Assembly assembly)
    {
        return assembly
            .GetCustomAttributes(false)
            .OfType<DebuggableAttribute>()
            .Any(attr => attr.IsJITTrackingEnabled);
}


    // private static string? GetPlatform()
    // {
    //     return IntPtr.Size switch
    //     {
    //         8 => "x64",
    //
    //         4 => "x86",
    //
    //         _ => null
    //     };
    // }
}