using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace PolyglotSql.Bundle
{
    internal static class LibPathResolver
    {
        public static string GetCurrentDllPath()
        {
            try
            {
                string location = typeof(LibPathResolver).Assembly.Location;
                if (!string.IsNullOrEmpty(location))
                    return location;
            }
            catch
            {
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsDllPath();
            }
            else
            {
                return GetUnixLibPath();
            }
        }

        #region Windows
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetModuleHandleEx(uint dwFlags, IntPtr lpModuleName, out IntPtr phModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, uint nSize);

        private static string GetWindowsDllPath()
        {
            // current method handle inside current dll
            IntPtr ptr = typeof(LibPathResolver).GetMethod(nameof(GetCurrentDllPath))!.MethodHandle.GetFunctionPointer();

            const uint GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS = 0x00000004;
            if (GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, ptr, out IntPtr hModule))
            {
                var sb = new StringBuilder(260);
                if (GetModuleFileName(hModule, sb, (uint)sb.Capacity) > 0) return sb.ToString();
            }
            return AppContext.BaseDirectory;
        }
        #endregion

        #region Unix (Linux/macOS)
        [StructLayout(LayoutKind.Sequential)]
        private struct Dl_info
        {
            public IntPtr dli_fname;
            public IntPtr dli_fbase;
            public IntPtr dli_sname;
            public IntPtr dli_saddr;
        }

        [DllImport("libdl", EntryPoint = "dladdr")]
        private static extern int dladdr(IntPtr addr, ref Dl_info info);

        private static string GetUnixLibPath()
        {
            // current method handle inside current so
            IntPtr ptr = typeof(LibPathResolver).GetMethod(nameof(GetCurrentDllPath))!.MethodHandle.GetFunctionPointer();
            Dl_info info = new Dl_info();
            if (dladdr(ptr, ref info) != 0 && info.dli_fname != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(info.dli_fname) ?? AppContext.BaseDirectory;
            }
            return AppContext.BaseDirectory;
        }
        #endregion
    }
}
