extern alias vanilla;

using System;
using System.Diagnostics;
using System.IO;
using Breach;

namespace OuterBeyond
{
    internal static class patch_Program
    {
        extern static void orig_Main(string[] args);

        internal static void Main(string[] args)
        {
            // TODO: Smarter error handling
            AppDomain.CurrentDomain.UnhandledException += (_, e) => HandleException((Exception)e.ExceptionObject);

            try
            {
                patch_THDebug._Initialize();
                Loader.Load();

                // Setup steam_appid.txt (Steam seems to look in the working directory)
                Directory.SetCurrentDirectory(new FileInfo(typeof(THGame).Assembly.Location).Directory.FullName);
                File.WriteAllText("steam_appid.txt", "332200");

                orig_Main(args);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        private static void HandleException(Exception e)
        {
            Console.WriteLine(e);
            try
            {
                vanilla::OuterBeyond.THDebug.PrintException(THLogPriority.FATAL, "Uncaught exception occurred while running the game.", e);
            }
            catch { }

            Debugger.Break();
            Environment.Exit(1);
        }
    }
}
