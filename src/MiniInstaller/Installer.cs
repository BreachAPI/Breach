using System;
using System.IO;

namespace Breach.MiniInstaller
{
    public static partial class Program
    {
        // TODO: Don't assmume dependencies are there, verify dependencies
        public static bool Install(DirectoryInfo folder, bool force = false)
        {
            LoadMonoMod();
            Directory.SetCurrentDirectory(folder.FullName);
            Log($"Starting Breach installation in '{folder.FullName}'");

            SetPaths(folder);

            try
            {
                // Check if already installed
                if (File.Exists(backupExe) && !force)
                {
                    Log("Breach is already installed.");
                    return false;
                }

                // TODO: check for existence of type MonoMod.WasHere to ensure the .exe is actually vanilla
                Log($"Backing up original exe: '{vanillaExe}' -> '{backupExe}'");
                File.Move(vanillaExe, backupExe);

                Log("Publicizing game classes...");
                var stats = PublicizeAssembly(backupExe, pubbedExe, false, false);
                File.Copy(pubbedExe, vanillaExe);
                Log($"Publicized {stats.TypeCount} types.");

                Log("Creating stripped game for mod dev...");
                PublicizeAssembly(backupExe, strippedExe, false, true);

                try
                {
                    Log("Running HookGen to generate hooks...");
                    hookgen.Invoke(null, new object[]
                    {
                        new[]
                        {
                            "--namespace", "On",
                            "--namespace-il", "OnIL",
                            "AxiomVerge.exe"
                        }
                    });
                    Log("HookGen completed.");
                }
                catch (Exception ex)
                {
                    Log($"HookGen failed: {ex}");
                    return false;
                }

                try
                {
                    Log("Running MonoMod patcher...");
                    patcher.Invoke(null, new object[]
                    {
                        new[]
                        {
                            "AxiomVerge.exe"
                        }
                    });
                    Log("MonoMod patcher completed.");
                }
                catch (Exception ex)
                {
                    Log($"MonoMod patcher failed: {ex}");
                    return false;
                }

                Log($"Replacing files...");
                File.Delete(vanillaExe);
                if (File.Exists(vanillaMdb)) File.Delete(vanillaMdb);
                if (File.Exists(vanillaPdb)) File.Delete(vanillaPdb);
                File.Move(moddedExe, vanillaExe);
                if (File.Exists(moddedMdb)) File.Move(moddedMdb, vanillaMdb);
                if (File.Exists(moddedPdb)) File.Move(moddedPdb, vanillaPdb);
                Directory.CreateDirectory(modsDirectory);

                Log("Installation completed successfully. Insert your mods in the Mods folder.");

                return true;
            }
            catch (Exception e)
            {
                Log(e.ToString());
                return false;
            }
        }
    }
}