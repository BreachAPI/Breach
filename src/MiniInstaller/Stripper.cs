using System;
using System.IO;

using StripConfig = System.Collections.Generic.List<(string SourceFile, string DestFile, bool PublicizeMembers, bool PublicizeType)>;

namespace Breach.MiniInstaller
{
    public static partial class Program
    {
        private static StripConfig stripConfig = new StripConfig() {
            ("AxiomVerge.exe", "pub_AxiomVerge.exe", true, true),
            ("AxiomVerge.exe", "AxiomVerge.exe", false, true),
            ("MMHOOK_AxiomVerge.dll", "MMHOOK_AxiomVerge.dll", false, false),
            ("Newtonsoft.Json.dll", "Newtonsoft.Json.dll", false, false),
            ("log4net.dll", "log4net.dll", false, false),
            ("FNA.dll", "FNA.dll", false, false),
        };

        public static bool Strip(DirectoryInfo folder)
        {
            LoadMonoMod();
            Directory.SetCurrentDirectory(folder.FullName);
            Log($"Stripping Axiom Verge binaries in '{folder.FullName}'...");

            SetPaths(folder);

            try
            {
                if (!Directory.Exists(stripDirectory)) Directory.CreateDirectory(stripDirectory);

                foreach (var config in stripConfig)
                {
                    var source = Path.Combine(folder.FullName, config.SourceFile);
                    if (File.Exists(source))
                    {
                        var dest = Path.Combine(stripDirectory, config.DestFile);
                        Log($"Stripping '{source}' -> '{dest}'");
                        PublicizeAssembly(source, dest, config.PublicizeMembers, true, config.PublicizeType);
                    }
                }

                Log("Stripping completed successfully. Find the stripped assemblies in the 'libs' folder.");

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