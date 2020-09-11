﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Isocrash.Net
{
    public static class FileSystem
    {
        private static string[] _specialDirectories
        {
            get
            {
                return new string[4]
                    {
                        Directory.GetCurrentDirectory(),
                        "/world/",
                        "/plugins/",
                        "/settings"
                    };
            }
        }

        public static string RootFolder
        {
            get { return Directory.GetCurrentDirectory(); }
        }
        
        private static Dictionary<string, string> _customDirectories = new Dictionary<string, string>();

        public static void AddCustomFolder(string internalName, string folderPath)
        {
            if (!_customDirectories.ContainsKey(internalName))
            {
                _customDirectories.Add(internalName, folderPath);
            }
            else
            {
                Server.LogWarning("Couldn't create custom folder \"" + internalName + "\": a custom folder already has this name.");
            }
        }

        public static DirectoryInfo GetCustomDirectory(string internalName)
        {
            string dir = null;
            foreach (KeyValuePair<string, string> kvp in _customDirectories)
            {
                if (kvp.Key == internalName)
                {
                    dir = RootFolder + kvp.Value;
                    break;
                }
            }

            if (String.IsNullOrEmpty(dir))
            {
                return null;
            }

            return Directory.CreateDirectory(dir);
        }

        public static DirectoryInfo GetSpecialDirectory(Folder specialFolder)
        {
            return Directory.CreateDirectory(RootFolder + _specialDirectories[(int) specialFolder]);
        }
    }
}