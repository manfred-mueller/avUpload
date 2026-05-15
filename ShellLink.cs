using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace avUpload
{
    /// <summary>
    /// Erstellt Windows-Verknüpfungen (.lnk) ohne COM-Referenz auf IWshRuntimeLibrary.
    /// Verwendet die native Shell32-Schnittstelle IShellLink direkt per P/Invoke.
    /// </summary>
    internal static class ShellLink
    {
        public static void CreateShortcut(
            string lnkPath,
            string targetPath,
            string workingDirectory,
            string description)
        {
            var link = (IShellLink)new CShellLink();

            link.SetPath(targetPath);
            link.SetWorkingDirectory(workingDirectory);
            link.SetDescription(description);

            ((IPersistFile)link).Save(lnkPath, fRemember: false);
        }

        // ------------------------------------------------------------------ //
        //  Native COM-Interfaces – kein separates Assembly nötig
        // ------------------------------------------------------------------ //

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class CShellLink { }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLink
        {
            void GetPath(
                [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszFile,
                int cch,
                out WIN32_FIND_DATA pfd,
                uint fFlags);

            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);

            void GetDescription(
                [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszName,
                int cchMaxName);

            void SetDescription(
                [MarshalAs(UnmanagedType.LPWStr)] string pszName);

            void GetWorkingDirectory(
                [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszDir,
                int cchMaxPath);

            void SetWorkingDirectory(
                [MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            void GetArguments(
                [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszArgs,
                int cchMaxPath);

            void SetArguments(
                [MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);

            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);

            void GetIconLocation(
                [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszIconPath,
                int cchIconPath,
                out int piIcon);

            void SetIconLocation(
                [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
                int iIcon);

            void SetRelativePath(
                [MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
                uint dwReserved);

            void Resolve(IntPtr hwnd, uint fFlags);

            void SetPath(
                [MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATA
        {
            public uint    dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint    nFileSizeHigh;
            public uint    nFileSizeLow;
            public uint    dwReserved0;
            public uint    dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string  cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string  cAlternateFileName;
        }
    }
}
