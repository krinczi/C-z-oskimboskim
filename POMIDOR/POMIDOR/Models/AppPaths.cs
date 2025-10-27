using System;
using System.IO;

namespace POMIDOR.Models
{
    public static class AppPaths
    {
        public static string BaseDir
        {
            get
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "POMIDOR");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string AnalyticsDir
        {
            get
            {
                var dir = Path.Combine(BaseDir, "analytics");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string ReportsDir
        {
            get
            {
                var dir = Path.Combine(BaseDir, "reports");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string PomodoroSessionsFile => Path.Combine(AnalyticsDir, "pomodoro_sessions.json");
    }
}
