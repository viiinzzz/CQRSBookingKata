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

public record ProgramInfo(string? ExeName, string ExeVersion, string BuildConfiguration, bool IsDebug)
{
    public string Print()
    {
        // var archi = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
        var archi = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower();

        return $"{ExeName} {ExeVersion} {BuildConfiguration} {archi}";
    }

    // public static string? GetPlatform()
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

    public static ProgramInfo Current 
        
        => GetProgramInfo(Assembly.GetEntryAssembly());

    public static (bool isDebug, bool isRelease, string programInfoStr) Get()
    {
        var pif = Current;
        var programInfoStr = pif.Print();

        var isDebug = pif.IsDebug;
        var isRelease = !isDebug;
        
        return (isDebug, isRelease, programInfoStr);
    }

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