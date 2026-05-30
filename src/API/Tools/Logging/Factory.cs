using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Breach.Tools
{
    public static partial class Log
    {
        public static ILog GetModLogger(string id, bool hasAppender = false)
        {
            var repo = (Hierarchy)LogManager.GetRepository();

            string loggerName = $"MOD.{id.ToUpperInvariant()}";
            var logger = repo.GetLogger(loggerName) as Logger;

            logger.Level = Level.Debug;
            logger.Additivity = true;

            // Avoid duplication
            if (logger.GetAppender($"Mod-{id}") == null)
            {
                var layout = new PatternLayout("%date [%level] - %message%newline");
                layout.ActivateOptions();

                var appender = new RollingFileAppender
                {
                    Name = $"Mod-{id}",
                    File = $"Logs/Mods/{id}.log",
                    AppendToFile = true,
                    StaticLogFileName = true,

                    MaximumFileSize = "1MB",
                    MaxSizeRollBackups = 5,
                    RollingStyle = RollingFileAppender.RollingMode.Size,

                    Layout = layout,
                    Threshold = Level.Debug
                };

                appender.ActivateOptions();
                logger.AddAppender(appender);
            }

            return LogManager.GetLogger(loggerName);
        }
    }
}