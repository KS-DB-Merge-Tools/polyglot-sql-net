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
            string rid = GetRid();
            string runtimeFolder = $"runtimes/{rid}/native";

            string currentDllDir = Path.GetDirectoryName(LibPathResolver.GetCurrentDllPath()) ?? AppContext.BaseDirectory;
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
        }


        private static string GetRid()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "win-x64";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string? ostype = Environment.GetEnvironmentVariable("OSTYPE");
                if (ostype == "linux-musl")
                    return "linux-musl-x64";
                return "linux-x64";
            }
            return "linux-x64";
        }

        private static string GetLibFileName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "polyglot_sql_ffi.dll";
            return "libpolyglot_sql_ffi.so";
        }
    }
}