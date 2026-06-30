using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PolyglotSql
{
    public static class NativeLibLoader
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("libdl", EntryPoint = "dlopen")]
        private static extern IntPtr UnixDlOpen(string filename, int flags);

        [DllImport("libdl", EntryPoint = "dlsym")]
        private static extern IntPtr UnixDlSym(IntPtr handle, string symbol);

        [DllImport("libdl", EntryPoint = "dlclose")]
        private static extern int UnixDlClose(IntPtr handle);

        [DllImport("libdl", EntryPoint = "dlerror")]
        private static extern IntPtr UnixDlError();

        private const int RTLD_NOW = 2;

        public static IntPtr Load(string libraryPath)
        {
            Console.WriteLine("[DEBUG] libraryPath: " + libraryPath);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr handle = LoadLibrary(libraryPath);
                if (handle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new DllNotFoundException($"Failed to load Windows DLL: {libraryPath}. Win32 Error: {errorCode}");
                }
                return handle;
            }
            else
            {
                IntPtr handle = UnixDlOpen(libraryPath, RTLD_NOW);
                if (handle == IntPtr.Zero)
                {
                    string errorMessage = Marshal.PtrToStringAnsi(UnixDlError()) ?? "Unknown error";
                    throw new DllNotFoundException($"Failed to load Unix library: {libraryPath}. dlopen Error: {errorMessage}");
                }
                return handle;
            }
        }

        public static IntPtr GetSymbol(IntPtr handle, string functionName)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentNullException(nameof(handle));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr address = GetProcAddress(handle, functionName);
                if (address == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new EntryPointNotFoundException($"Function '{functionName}' not found. Win32 Error: {errorCode}");
                }
                return address;
            }
            else
            {
                IntPtr address = UnixDlSym(handle, functionName);
                if (address == IntPtr.Zero)
                {
                    string errorMessage = Marshal.PtrToStringAnsi(UnixDlError()) ?? "Unknown error";
                    throw new EntryPointNotFoundException($"Function '{functionName}' not found. dlsym Error: {errorMessage}");
                }
                return address;
            }
        }

        public static void Free(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                FreeLibrary(handle);
            else
                UnixDlClose(handle);
        }
    }
}
