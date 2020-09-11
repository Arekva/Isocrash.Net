﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Isocrash.Net
{
    public class Plugin
    {
        internal static List<Plugin> Plugins = new List<Plugin>();

        public static Plugin Get(string name)
        {
            foreach(Plugin plugin in Plugins)
                if (plugin.Name == name)
                    return plugin;

            return null;
        }

        internal static void Add(Plugin plugin)
        {
            if(Get(plugin.Name) == null) Plugins.Add(plugin);
        }
        
        internal Assembly CorrespondingAssembly { get; }

        public string Name { get; } = "Third Party Plugin";

        internal Plugin(string name, Assembly assembly)
        {
            this.Name = name;
            this.CorrespondingAssembly = assembly;
            Add(this);
        }

        internal static Plugin[] LoadPluginsInFolder(string directory)
        {
            string dirPath = directory;//FileSystem.RootFolder + directory;

            string[] files = Directory.GetFiles(dirPath, "*.dll", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    Assembly pluginAssembly = Assembly.LoadFrom(files[i]);

                    
                    
                    FileVersionInfo pluginVersion = FileVersionInfo.GetVersionInfo(pluginAssembly.Location);
                    Plugin p = new Plugin(pluginVersion.OriginalFilename, pluginAssembly);
                    
                    //Console.WriteLine("Loaded plugin " + p.Name + " " + pluginVersion.ProductVersion);
                }

                catch (Exception e)
                {
                    Console.WriteLine("Unable to load plugin :" + Environment.NewLine + e);
                }
            }

            return Plugins.ToArray();
        }
    }
}