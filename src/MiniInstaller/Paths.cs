using System.IO;
using System.Reflection;

namespace Breach.MiniInstaller
{
    public static partial class Program
    {
        private static MethodInfo patcher;
        private static MethodInfo hookgen;

        /// <summary>
        /// Loads the patcher and hook generator assemblies
        /// </summary>
        public static void LoadMonoMod()
        {
            // God bless
            // TODO: Assembly and reflection error handling

            var currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

            if (patcher is null)
            {
                var patcherExe = Path.Combine(currentFolder.FullName, "MonoMod.exe");
                // TODO: Upgrade to MonoMod.Patcher (v25 I believe)
                var patcherDll = Path.Combine(currentFolder.FullName, "MonoMod.dll");
                var patcherContents = File.Exists(patcherExe) ? File.ReadAllBytes(patcherExe) : File.ReadAllBytes(patcherDll);
                patcher = Assembly.Load(patcherContents).GetType("MonoMod.Program").GetMethod("Main");
            }

            if (hookgen is null)
            {
                var hookgenExe = Path.Combine(currentFolder.FullName, "MonoMod.RuntimeDetour.HookGen.exe");
                var hookgenDll = Path.Combine(currentFolder.FullName, "MonoMod.RuntimeDetour.HookGen.dll");
                var hookgenContents = File.Exists(hookgenExe) ? File.ReadAllBytes(hookgenExe) : File.ReadAllBytes(hookgenDll);
                hookgen = Assembly.Load(hookgenContents).GetType("MonoMod.RuntimeDetour.HookGen.Program").GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
            }
        }

        private static string vanillaExe;
        private static string vanillaMdb;
        private static string vanillaPdb;
        private static string backupExe;
        private static string strippedExe;
        private static string pubbedExe;
        private static string moddedExe;
        private static string moddedPdb;
        private static string moddedMdb;
        private static string hooksDll;
        private static string modsDirectory;
        private static string stripDirectory;
        private static readonly string[] _uninstallerFiles = new string[]
        {
            "AxiomVerge.Breach.mm.dll",
            "AxiomVerge.Breach.mm.pdb",
            "Breach.API.dll",
            "Breach.API.pdb",
            "MonoMod.RuntimeDetour.dll",
            "MonoMod.Utils.dll",
            "Mono.Cecil.Rocks.dll",
            "Mono.Cecil.Pdb.dll",
            "Mono.Cecil.Mdb.dll",
            "Mono.Cecil.dll",
        };
        private static string[] uninstallerFiles;

        public static void SetPaths(DirectoryInfo folder)
        {
            vanillaExe = Path.Combine(folder.FullName, "AxiomVerge.exe");
            vanillaPdb = Path.Combine(folder.FullName, "AxiomVerge.exe.pdb");
            vanillaMdb = Path.Combine(folder.FullName, "AxiomVerge.exe.mdb");
            backupExe = Path.Combine(folder.FullName, "orig_AxiomVerge.exe");
            strippedExe = Path.Combine(folder.FullName, "strip_AxiomVerge.exe");
            pubbedExe = Path.Combine(folder.FullName, "pub_AxiomVerge.exe");
            moddedExe = Path.Combine(folder.FullName, "MONOMODDED_AxiomVerge.exe");
            moddedPdb = Path.Combine(folder.FullName, "MONOMODDED_AxiomVerge.exe.pdb");
            moddedMdb = Path.Combine(folder.FullName, "MONOMODDED_AxiomVerge.exe.mdb");
            hooksDll = Path.Combine(folder.FullName, "MMHOOK_AxiomVerge.dll");
            modsDirectory = Path.Combine(folder.FullName, "Mods");
            stripDirectory = Path.Combine(folder.FullName, "libs");

            uninstallerFiles = new string[_uninstallerFiles.Length];
            for (int i = 0; i < _uninstallerFiles.Length; i++)
            {
                uninstallerFiles[i] = Path.Combine(folder.FullName, _uninstallerFiles[i]);
            }
        }
    }
}