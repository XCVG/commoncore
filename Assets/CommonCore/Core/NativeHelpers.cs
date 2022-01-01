using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CommonCore
{
    /// <summary>
    /// P/Invoke and other native helpers
    /// </summary>
    internal static class NativeHelpers
    {
        [DllImport("shell32.dll")]
        private extern static int SHGetKnownFolderPath(ref Guid folderId, uint flags, IntPtr token, [MarshalAs(UnmanagedType.LPWStr)] out string pszPath);

        internal static string GetSavedGamesFolderPath()
        {
            var guid = Guid.Parse("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"); //saved games folder GUID
            int result = SHGetKnownFolderPath(ref guid, 0, IntPtr.Zero, out string path);
            if(result != 0)
            {
                throw new Win32Exception(result);
            }
            return path;
        }

    }

}