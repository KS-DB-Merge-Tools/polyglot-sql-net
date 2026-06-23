using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using PolyglotSql;

namespace SqlGlotDotNet.DebugTest;

public static class NativeResolver
{
    private static bool _registered = false;

    public static void Register()
    {
        if (_registered) return;
        _registered = true;
        NativeLibrary.SetDllImportResolver(typeof(PolyglotNative).Assembly, DllImportResolver);
    }

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != "polyglot_sql_ffi")
            return IntPtr.Zero;

        string libFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "polyglot_sql_ffi.dll"
            : "libpolyglot_sql_ffi.so";

        string runtimeFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "runtimes/win-x64/native"
            : "runtimes/linux-x64/native";

        string[] candidates =
        {
            Path.Combine(Environment.CurrentDirectory, runtimeFolder, libFileName),
            Path.Combine(Environment.CurrentDirectory, libFileName),
            Path.Combine(AppContext.BaseDirectory, runtimeFolder, libFileName),
            Path.Combine(AppContext.BaseDirectory, libFileName)
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                try
                {
                    return NativeLibrary.Load(path);
                }
                catch
                {
                }
            }
        }

        return NativeLibrary.Load(libraryName, assembly, searchPath);
    }
}
