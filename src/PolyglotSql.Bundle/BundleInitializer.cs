using PolyglotSql;
using System.IO;
using System.Runtime.InteropServices;
using System;

namespace PolyglotSql.Bundle
{
    public static class BundleInitializer
    {
        public static void Initialize()
        {
            string libPath = GetLibPath();
            // Console.WriteLine("[DEBUG] libPath: " + libPath);
            var provider = new DynamicRustProvider(libPath);
            Polyglot.RegisterProvider(provider);
        }

        private static string GetLibPath()
        {
            string libFileName = GetLibFileName();
            string libFolder = Path.GetDirectoryName(LibPathResolver.GetCurrentDllPath());

#if NET8_0_OR_GREATER
            return Path.Combine(libFolder, libFileName);
#else
            string rid = GetWindowsRid();
            string runtimeFolder = $"runtimes/{rid}/native";

            string currentDllDir = AppContext.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(currentDllDir, runtimeFolder, libFileName),
                Path.Combine(currentDllDir, libFileName),
            };

            foreach (var path in candidates)
            {
                // Console.WriteLine("[DEBUG] path: " + path);
                if (File.Exists(path))
                    return path;
            }

            return libFileName;
#endif
        }


        private static string GetWindowsRid()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                return "win-x86";

            return "win-x64";
        }

        private static string GetLibFileName()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "polyglot_sql_ffi.dll"
                : "libpolyglot_sql_ffi.so";
        }
    }
}