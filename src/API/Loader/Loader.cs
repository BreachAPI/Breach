using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Breach.Tools;

namespace Breach
{
    public static class Loader
    {
        public static readonly Assembly LoaderAssembly = typeof(Loader).Assembly;
        public static readonly string PathGameFile;
        public static readonly string PathGameFolder;
        public static readonly string PathModsFolder;

        public static ImmutableArray<BreachMod> Mods { get; private set; } = ImmutableArray<BreachMod>.Empty;
        public static ImmutableArray<BreachModule> Modules { get; private set; } = ImmutableArray<BreachModule>.Empty;

        static Loader()
        {
            PathGameFile = Path.GetFullPath(typeof(OuterBeyond.THGame).Assembly.Location);
            PathGameFolder = Path.GetDirectoryName(PathGameFile)!;
            PathModsFolder = Path.Combine(PathGameFolder, "Mods");

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }


        // Needed to resolve referenced assemblies correctly on mono
        private static Assembly ResolveAssembly(object o, ResolveEventArgs e)
        {
            if (e.RequestingAssembly is null) return null;

            var requestingFile = new FileInfo(e.RequestingAssembly.Location);
            var requestingDir = requestingFile.Directory;
            if (requestingDir is null) return null;

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
            if (!Directory.Exists(PathModsFolder))
            {
                // TODO: Log Mods folder missing and create it
                return;
            }

            // Find and parse all metadata files
            var toLoad = FindMods();
            // Ensure all mod Ids are unique and all dependencies are present
            VerifyMetadata(toLoad);
            toLoad = OrderModsByDependencies(toLoad);

            var loadedMods = LoadMods(toLoad);
            Mods = loadedMods.ToImmutableArray();
            var loadedModules = loadedMods.SelectMany(m => m.Modules).ToImmutableArray();
            Modules = loadedModules;
            
            foreach (var module in loadedModules) {
                module.Initialize();
            }
        }

        private static List<BreachModHandle> FindMods() {
            var list = new List<BreachModHandle>();

            foreach (var modDir in Directory.EnumerateDirectories(PathModsFolder)) {
                var metadataFile = new FileInfo(Path.Combine(modDir, "mod.json"));
                if (!metadataFile.Exists) continue;

                list.Add(new BreachModHandle {
                    Metadata = ParseMetadata(metadataFile),
                    Path = modDir
                });
            }

            return list;
        }

        public static BreachModMetadata ParseMetadata(FileInfo metadataFile) {
            // TODO: load metadata json
            return new BreachModMetadata();
        }

        private static void VerifyMetadata(List<BreachModHandle> modList) {
            bool hasIdConflicts = false;
            var knownIds = new Dictionary<string, BreachModHandle>();
            foreach (var modHandle in modList) {
                if (knownIds.TryGetValue(modHandle.Metadata.Id, out var other)) {
                    hasIdConflicts = true;
                    // TODO: Log mod conflict (including file paths)
                }

                knownIds.Add(modHandle.Metadata.Id, modHandle);
            }

            if (hasIdConflicts) {
                throw new Exception("mod load error: multiple mods with same id");
            }

            var missingDependencies = new List<Tuple<string, BreachModHandle>>();
            foreach (var modHandle in modList) {
                foreach (var dependency in modHandle.Metadata.Dependencies) {
                    if (!knownIds.ContainsKey(dependency)) {
                        missingDependencies.Add(new(dependency, modHandle));
                        // TODO: Log missing dependency (mod ID, dep ID)
                    }
                }
            }

            if (missingDependencies.Count > 0) {
                throw new Exception("mod load error: missing dependencies");
            }
        }

        private static List<BreachModHandle> OrderModsByDependencies(List<BreachModHandle> modList) {
            var loadOrder = new List<BreachModHandle>();
            var processedMods = new HashSet<string>();
            var remainingMods = new List<BreachModHandle>(modList.Count);

            // Default to loading in order of Id, to ensure consistent load order
            remainingMods.AddRange(modList.OrderBy(m => m.Metadata.Id));

            while (loadOrder.Count < remainingMods.Count) {
                int orderedCountBefore = loadOrder.Count;
                for (int i = 0; i < remainingMods.Count; i++) {
                    // We set entries to null when we moved them to loadOrder, so skip nulls
                    if (remainingMods[i] == null) continue;

                    var currentMod = remainingMods[i];
                    bool allDependenciesPresent = true;
                    foreach (var dependency in currentMod.Metadata.Dependencies) {
                        if (!processedMods.Contains(dependency)) {
                            allDependenciesPresent = false;
                        }
                    }

                    if (allDependenciesPresent) {
                        loadOrder.Add(currentMod);
                        processedMods.Add(currentMod.Metadata.Id);
                        // Don't remove so we don't have to worry about shifting indices.
                        remainingMods[i] = null;
                    }
                }

                if (loadOrder.Count == orderedCountBefore) {
                    // We didn't find a single mod without missing dependencies.
                    // This means there is a dependency loop (or multiple)!
                    // TODO: Log all mods still in remainingMods
                    throw new Exception("mod load error: circular dependency");
                }
            }

            return loadOrder;
        }

        private static List<BreachMod> LoadMods(List<BreachModHandle> modList) {
            var loadedModList = new List<BreachMod>();
            var loadOrder = modList.Select(m => m.Metadata).ToImmutableList();

            foreach (var mod in modList) {
                var moduleList = new List<BreachModule>();

                foreach (var file in mod.Metadata.Assemblies) {
                    var assembly = Assembly.LoadFrom(Path.Combine(mod.Path, file));

                    foreach (var module in assembly.GetModules())
                    {
                        foreach (var type in module.GetTypes())
                        {
                            // Skip types that do not derive from BreachModule
                            if (!typeof(BreachModule).IsAssignableFrom(type)) continue;
                            // Skip types that are not marked with the [AutoLoad] attribute
                            var attr = type.GetCustomAttribute<AutoLoadAttribute>();
                            if (attr is null) continue;

                            // Instantiate BreachModule
                            var instance = (BreachModule)Activator.CreateInstance(type);
                            instance.Logger = Log.GetModLogger(attr.LoggerName ?? file, attr.HasOwnLogFile);
                            moduleList.Add(instance);
                        }
                    }
                }

                for (int i = 0; i < moduleList.Count; i++) {
                    var module = moduleList[i];
                    var register = module.RegisterModules(loadOrder);
                    moduleList.AddRange(register);
                }

                loadedModList.Add(new() {
                    Metadata = mod.Metadata,
                    Path = mod.Path,
                    Modules = moduleList.ToImmutableArray()
                });
            }

            return loadedModList;
        }
    }
}
