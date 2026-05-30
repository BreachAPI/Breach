using System.IO;
using log4net;
using log4net.Config;

namespace Breach.Tools
{
    public static partial class Log
    {
        private static ILog Vanilla;
        internal static ILog Breach;
        public static ILog Debug;

        public static ILog Initialize()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("LogAppender.Breach.xml"));

            Vanilla = LogManager.GetLogger("VANILLA");
            Breach = LogManager.GetLogger("BREACH");
            Debug = LogManager.GetLogger("DEBUG");

            return Vanilla;
        }
    }
}