using System;
using System.IO;

namespace Breach.MiniInstaller
{
    public static partial class Program
    {
        public static bool Reinstall(DirectoryInfo folder)
        {
            LoadMonoMod();
            Directory.SetCurrentDirectory(folder.FullName);
            Log($"Starting Breach reinstallation in '{folder.FullName}'");

            SetPaths(folder);

            try
            {
                if (!File.Exists(backupExe))
                {
                    Log("Breach is not installed, installing from anew...");
                    return Install(folder);
                }

                Uninstall(folder, true);
                Install(folder, true);

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