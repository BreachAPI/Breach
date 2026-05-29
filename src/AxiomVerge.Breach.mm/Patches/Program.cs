using System;
using System.Diagnostics;
using System.IO;
using Breach;

namespace OuterBeyond
{
    internal static class Program
    {
        extern static void orig_Main(string[] args);

        private static void Main(string[] args)
        {
            // TODO: Smart error handling once logging is implemented
            AppDomain.CurrentDomain.UnhandledException += (_, e) => HandleException((Exception)e.ExceptionObject);

            try
            {
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
                THDebug.Print(e.ToString());
            }
            catch { }
            Debugger.Break();
            Environment.Exit(1);
        }
    }
}
