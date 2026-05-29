using System;
using System.IO;

namespace Breach.MiniInstaller
{
    public static partial class Program
    {
        public static void Log(string message)
        {
            Console.WriteLine($"[Breach.MiniInstaller] {message}");
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Log("No arguments provided. Pass 'install', 'reinstall', or 'uninstall' along with the game's folder to perform the corresponding operation.");
                return;
            }

            var dir = new DirectoryInfo(args[0]);
            if (dir.Exists)
            {
                Install(dir);
                return;
            }

            if (args.Length == 1)
            {
                Log("Provide the game's folder as the second argument");
            }

            dir = new DirectoryInfo(args[1]);

            switch (args[0].ToLowerInvariant())
            {
                case "install":
                case "i":
                    Install(dir);
                    break;
                case "reinstall":
                case "r":
                    Reinstall(dir);
                    break;
                case "uninstall":
                case "u":
                    Uninstall(dir);
                    break;
                case "strip":
                case "s":
                    Strip(dir);
                    break;
                default:
                    Log("Unrecognized operation. Supported operations: 'install', 'reinstall', 'uninstall'");
                    break;
            }
        }
    }
}
