using System;
using NLog;

namespace CenturyLink.Network.Engineering.Common.Utility
{
    public class DateTimeGenerator
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string runTimeZone = null;
        private static string timeZoneConfigurationKey = "timeZone";

        static DateTimeGenerator()
        {
            runTimeZone = System.Configuration.ConfigurationManager.AppSettings[timeZoneConfigurationKey];

            logger.Info("Time zone = " + runTimeZone);
            logger.Info("Local time zone: " + TimeZoneInfo.Local.DisplayName + "/" + TimeZoneInfo.Local.StandardName + "/" + TimeZoneInfo.Local.Id);
        }

        public static DateTime Now
        {
            get
            {
                try
                {
                    return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(runTimeZone));
                }
                catch (Exception ex)
                {
                    try
                    {
                        return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local);
                    }
                    catch (Exception ex2)
                    {
                        logger.Info(ex2, "Failure generating Time Zone based date");

                        return DateTime.Now;
                    }
                }
            }
        }
    }
}
