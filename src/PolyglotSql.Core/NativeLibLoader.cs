using System;
using System.Runtime.InteropServices;

namespace PolyglotSql
{
    internal static class NativeLibLoader
    {
        // We intentionally don't use NativeLibrary.Load for net8+ for now
        // because it would be one more thing that's needs to be covered with tests

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);


        [DllImport("libdl", EntryPoint = "dlopen")]
        private static extern IntPtr UnixDlOpenV1(string filename, int flags);

        [DllImport("libdl", EntryPoint = "dlsym")]
        private static extern IntPtr UnixDlSymV1(IntPtr handle, string symbol);

        [DllImport("libdl", EntryPoint = "dlclose")]
        private static extern int UnixDlCloseV1(IntPtr handle);

        [DllImport("libdl", EntryPoint = "dlerror")]
        private static extern IntPtr UnixDlErrorV1();


        [DllImport("libdl.so.2", EntryPoint = "dlopen")]
        private static extern IntPtr UnixDlOpenV2(string filename, int flags);

        [DllImport("libdl.so.2", EntryPoint = "dlsym")]
        private static extern IntPtr UnixDlSymV2(IntPtr handle, string symbol);

        [DllImport("libdl.so.2", EntryPoint = "dlclose")]
        private static extern int UnixDlCloseV2(IntPtr handle);

        [DllImport("libdl.so.2", EntryPoint = "dlerror")]
        private static extern IntPtr UnixDlErrorV2();


        private static bool UseUnixV2 = true;
        private const int RTLD_NOW = 2;

        public static IntPtr Load(string libraryPath)
        {
            // Console.WriteLine("[DEBUG] libraryPath: " + libraryPath);

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
                IntPtr handle;
                try
                {
                    handle = UnixDlOpenV2(libraryPath, RTLD_NOW);
                }
                catch (DllNotFoundException)
                {
                    UseUnixV2 = false;
                    handle = UnixDlOpenV1(libraryPath, RTLD_NOW);
                }

                if (handle == IntPtr.Zero)
                {
                    IntPtr error = UseUnixV2 ? UnixDlErrorV2() : UnixDlErrorV1();
                    string errorMessage = Marshal.PtrToStringAnsi(error) ?? "Unknown error";
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
                if (UseUnixV2)
                {
                    IntPtr address = UnixDlSymV2(handle, functionName);
                    if (address == IntPtr.Zero)
                    {
                        string errorMessage = Marshal.PtrToStringAnsi(UnixDlErrorV2()) ?? "Unknown error";
                        throw new EntryPointNotFoundException($"Function '{functionName}' not found. dlsym Error: {errorMessage}");
                    }
                    return address;
                }
                else
                {
                    IntPtr address = UnixDlSymV1(handle, functionName);
                    if (address == IntPtr.Zero)
                    {
                        string errorMessage = Marshal.PtrToStringAnsi(UnixDlErrorV1()) ?? "Unknown error";
                        throw new EntryPointNotFoundException($"Function '{functionName}' not found. dlsym Error: {errorMessage}");
                    }
                    return address;
                }
            }
        }

        public static void Free(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FreeLibrary(handle);
            }
            else
            {
                if (UseUnixV2)
                    UnixDlCloseV2(handle);
                else
                    UnixDlCloseV1(handle);
            }
        }
    }
}
