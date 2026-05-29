using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Breach
{
    public static class Loader
    {
        public static readonly Assembly LoaderAssembly = typeof(Loader).Assembly;
        public static readonly string PathGameFile;
        public static readonly string PathGameFolder;
        public static readonly string PathModsFolder;

        public static List<BreachModule> Modules = new List<BreachModule>();

        static Loader()
        {
            PathGameFile = Path.GetFullPath(typeof(OuterBeyond.THGame).Assembly.Location);
            PathGameFolder = Path.GetDirectoryName(PathGameFile);
            PathModsFolder = Path.Combine(PathGameFolder, "Mods");

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }


        // Needed to resolve referenced assemblies correctly on mono
        private static Assembly ResolveAssembly(object o, ResolveEventArgs e)
        {
            if (e.RequestingAssembly is null) return null;

            var requestingFile = new FileInfo(e.RequestingAssembly.Location);
            var requestingDir = requestingFile.Directory;

            var assemblyName = new AssemblyName(e.Name);

            var targetPath = Path.Combine(requestingDir.FullName, assemblyName + ".dll");

            if (!File.Exists(targetPath))
            {
                return null;
            }

            return Assembly.LoadFrom(targetPath);
        }

        public static void Load()
        {
            if (Directory.Exists(PathModsFolder))
            {
                var info = new DirectoryInfo(PathModsFolder);
                foreach (var file in info.EnumerateFiles("*.Mod.dll", SearchOption.TopDirectoryOnly))
                {
                    var assembly = Assembly.LoadFrom(file.FullName);

                    foreach (var module in assembly.GetModules())
                    {
                        foreach (var type in module.GetTypes())
                        {
                            // Skip types that are not marked with the [BreachModule] attribute
                            var attr = type.GetCustomAttribute<BreachModuleAttribute>();
                            if (attr == null) continue;

                            // Skip types that do not derive from BreachModule
                            bool hasBreachModuleParent = false;
                            var parentType = type.BaseType;
                            while (parentType != null)
                            {
                                if (parentType == typeof(BreachModule))
                                {
                                    hasBreachModuleParent = true;
                                    break;
                                }

                                parentType = type.BaseType;
                            }
                            if (!hasBreachModuleParent) continue;

                            // Create instance and go to town
                            BreachModule instance = (BreachModule)Activator.CreateInstance(type);
                            Modules.Add(instance);
                            instance.Initialize();
                        }
                    }
                }
            }

            Directory.SetCurrentDirectory(PathGameFolder);
        }
    }
}
