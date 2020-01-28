using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.ApplicationBlocks.ExceptionManagement;

namespace CenturyLink.Network.Engineering.Common.Logging
{
    public enum SentryIdentifier
    {
        Null = 0,
        EmailDev = 1001,
        EmailTierII = 1002,
        EmailDba = 1003,
        PageDev = 2001,
        PageTierII = 2002,
        PageDba = 2003,
        Ignore = 3000
    };

    public enum SentrySeverity
    {
        Critical = 1,
        Major = 2,
        Minor = 3,
        Warning = 4,
        Normal = 5,
        Info,
        Debug,
        Unknown
    }

    public enum LogType { Debug, Info, TierII_Info, Exception, Alarm };

    public class EventLogger
    {
        private static bool debugLogging = false;
        private static bool sentryLogging = false;
        private static Thread debugLoggingRefreshThread = new Thread(new ThreadStart(DebugLoggingRefreshWorker));
        private const int LOG_LEVEL_REFRESH_MINS = 5;
        private static string _sentrySubSystemName;

        static EventLogger()
        {
            RefreshDebugLogging();
            EnableAllSentryLogging();
        }

        private static void DebugLoggingRefreshWorker()
        {
            while (true)
            {
                RefreshDebugLogging();
                Thread.Sleep(LOG_LEVEL_REFRESH_MINS * 60 * 1000);
            }
        }

        private static void RefreshDebugLogging()
        {
            try
            {
                debugLogging = bool.Parse(ConfigurationManager.AppSettings["DebugLogging"]);
            }
            catch (Exception ex)
            {
                //LogException(ex);
            }
        }

        private static void EnableAllSentryLogging()
        {
            try
            {
                sentryLogging = bool.Parse(ConfigurationManager.AppSettings["EnableAllSentryLogging"]);
            }
            catch (Exception ex)
            {
                //LogException(ex);
            }
        }

        public static void TestLog(string message) //, Logger nlog)
        {
            //nlog.Info(message);

            Exception exception = new Exception("This is a test!!!!!!!");

            LogEvent(GetNameValueCollection(LogType.Alarm), exception, message, SentryIdentifier.EmailDev, SentrySeverity.Normal);
        }

        public static void LogDebug(string message)
        {
            if (debugLogging)
                LogEvent(GetNameValueCollection(LogType.Debug), null, message, SentryIdentifier.Ignore, SentrySeverity.Normal);
        }

        public static void LogInfo(string message)
        {
            LogEvent(GetNameValueCollection(LogType.Info), null, message, SentryIdentifier.Ignore, SentrySeverity.Normal);
        }

        public static void LogTierIIInfo(string message)
        {
            LogEvent(GetNameValueCollection(LogType.TierII_Info), null, message, SentryIdentifier.Ignore, SentrySeverity.Normal);
        }

        public static void LogException(Exception exception)
        {
            LogEvent(GetNameValueCollection(LogType.Exception), exception, "", SentryIdentifier.Ignore, SentrySeverity.Normal);
        }

        public static void LogException(Exception exception, string message)
        {
            LogEvent(GetNameValueCollection(LogType.Exception), exception, message, SentryIdentifier.Ignore, SentrySeverity.Normal);
        }

        public static void LogAlarm(Exception exception, string message, SentryIdentifier sentryIdentifier, SentrySeverity sentrySeverity)
        {
            LogEvent(GetNameValueCollection(LogType.Alarm), exception, message, sentryIdentifier, sentrySeverity);
        }

        private static NameValueCollection GetNameValueCollection(LogType logType)
        {
            NameValueCollection additionalInfo = new NameValueCollection();

            additionalInfo.Add("LogType", logType.ToString());

            return additionalInfo;
        }

        private static void LogEvent(NameValueCollection additionalInfo, Exception exception, string message, SentryIdentifier sentryIdentifier, SentrySeverity sentrySeverity)
        {
            LogType logType = (LogType)Enum.Parse(typeof(LogType), additionalInfo.Get("LogType"));
            string classMethodInfo = "";
            StackTrace stack = new StackTrace(true);

            //if (projectId > 0)
                //additionalInfo.Add(DBLogger.PROJECT_ID, projectId.ToString());

            if (exception == null)
            {
                switch (logType)
                {
                    case LogType.Debug:
                        exception = new Exception("Debug Event");
                        break;
                    case LogType.Info:
                        exception = new Exception("Information Event");
                        break;
                    case LogType.TierII_Info:
                        exception = new Exception("Information Event");
                        break;
                    default:
                        exception = new Exception("No Exception");
                        break;
                }
            }

            if (logType == LogType.Alarm || sentryLogging)
            {
                additionalInfo.Add("sentry.severity", sentrySeverity.ToString());
                additionalInfo.Add("sentry.identifier", ((int)sentryIdentifier).ToString());
                additionalInfo.Add("sentry.subsystem", SentrySubSystemName);
            }

            for (int fIndex = 2; fIndex < stack.FrameCount; fIndex++)
            {
                string fName = stack.GetFrame(fIndex).GetMethod().DeclaringType.FullName;

                if (typeof(EventLogger).FullName == fName)
                    continue;
                else
                {
                    ArrayList methodParamsList = new ArrayList();
                    ParameterInfo[] parameters = stack.GetFrame(fIndex).GetMethod().GetParameters();

                    foreach (ParameterInfo pi in parameters)
                    {
                        methodParamsList.Add(pi.ParameterType.Name);
                    }

                    classMethodInfo = stack.GetFrame(fIndex).GetMethod().DeclaringType.FullName + "." +
                        stack.GetFrame(fIndex).GetMethod().Name +
                        "(" + string.Join(",", ((string[])methodParamsList.ToArray(typeof(string)))) + ")";

                    if (classMethodInfo.Length > 0)
                        additionalInfo.Add("sentry.classinfo", classMethodInfo);

                    break;
                }
            }

            additionalInfo.Add("sentry.message", message);

            ExceptionManager.Publish(exception, additionalInfo);
        }

        private static long LogEvent(NameValueCollection additionalInfo, Exception exception, string message, SentryIdentifier sentryIdentifier, SentrySeverity sentrySeverity, string dummy)
        {
            LogType logType = (LogType)Enum.Parse(typeof(LogType), additionalInfo.Get("LogType"));
            string classMethodInfo = "";
            StackTrace stack = new StackTrace(true);

            //if (projectId > 0)
                //additionalInfo.Add(DBLogger.PROJECT_ID, projectId.ToString());

            if (exception == null)
            {
                switch (logType)
                {
                    case LogType.Debug:
                        exception = new Exception("Debug Event");
                        break;
                    case LogType.Info:
                        exception = new Exception("Information Event");
                        break;
                    case LogType.TierII_Info:
                        exception = new Exception("Information Event");
                        break;
                    default:
                        exception = new Exception("No Exception");
                        break;
                }
            }

            if (logType == LogType.Alarm || sentryLogging)
            {
                additionalInfo.Add("sentry.severity", sentrySeverity.ToString());
                additionalInfo.Add("sentry.identifier", ((int)sentryIdentifier).ToString());
                additionalInfo.Add("sentry.subsystem", SentrySubSystemName);
            }

            for (int fIndex = 2; fIndex < stack.FrameCount; fIndex++)
            {
                string fName = stack.GetFrame(fIndex).GetMethod().DeclaringType.FullName;

                if (typeof(EventLogger).FullName == fName)
                    continue;
                else
                {
                    ArrayList methodParamsList = new ArrayList();
                    ParameterInfo[] parameters = stack.GetFrame(fIndex).GetMethod().GetParameters();

                    foreach (ParameterInfo pi in parameters)
                    {
                        methodParamsList.Add(pi.ParameterType.Name);
                    }

                    classMethodInfo = stack.GetFrame(fIndex).GetMethod().DeclaringType.FullName + "." +
                        stack.GetFrame(fIndex).GetMethod().Name +
                        "(" + string.Join(",", ((string[])methodParamsList.ToArray(typeof(string)))) + ")";

                    if (classMethodInfo.Length > 0)
                        additionalInfo.Add("sentry.classinfo", classMethodInfo);

                    break;
                }
            }

            additionalInfo.Add("sentry.message", message);

            return ExceptionManager.Publish(exception, additionalInfo, "");
        }

        private static string SentrySubSystemName
        {
            get
            {
                if (_sentrySubSystemName == null)
                {
                    ExceptionManagementSettings exceptionManagementSettings = (ExceptionManagementSettings)ConfigurationManager.GetSection("exceptionManagement");

                    if (exceptionManagementSettings != null)
                    {
                        System.Collections.ArrayList publishersList = exceptionManagementSettings.Publishers;

                        foreach (PublisherSettings publisher in publishersList)
                        {
                            foreach (string publisherKey in publisher.OtherAttributes.AllKeys)
                            {
                                if (publisherKey.ToLower() == "sentry.subsystem")
                                {
                                    _sentrySubSystemName = publisher.OtherAttributes[publisherKey];
                                    return _sentrySubSystemName;
                                }
                            }
                        }
                    }
                }

                return _sentrySubSystemName;
            }
        }
    }
}
