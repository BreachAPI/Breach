using System;
using System.IO;

namespace Breach.MiniInstaller
{
    public static partial class Program
    {
        public static bool Uninstall(DirectoryInfo folder, bool partial = false)
        {
            LoadMonoMod();
            Directory.SetCurrentDirectory(folder.FullName);
            if (!partial) Log($"Starting Breach uninstallation in '{folder.FullName}'");

            SetPaths(folder);

            try
            {
                if (!File.Exists(backupExe))
                {
                    Log("Breach is already not installed.");
                }

                Log($"Removing modded files...");
                if (File.Exists(vanillaExe)) File.Delete(vanillaExe);
                if (File.Exists(vanillaMdb)) File.Delete(vanillaMdb);
                if (File.Exists(vanillaPdb)) File.Delete(vanillaPdb);
                if (File.Exists(hooksDll)) File.Delete(hooksDll);
                if (File.Exists(strippedExe)) File.Delete(strippedExe);
                if (File.Exists(pubbedExe)) File.Delete(pubbedExe);

                if (!partial)
                {
                    foreach (var uninstallerFile in uninstallerFiles)
                        if (File.Exists(uninstallerFile)) File.Delete(uninstallerFile);
                }

                Log($"Restoring original exe: '{backupExe}' -> '{vanillaExe}'");
                File.Move(backupExe, vanillaExe);

                Log("Uninstallation completed successfully. Mods folder has not been deleted.");

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