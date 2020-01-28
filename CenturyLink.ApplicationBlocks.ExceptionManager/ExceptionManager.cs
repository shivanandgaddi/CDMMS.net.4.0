//=============================================================================
// Class  : ExceptionManager      
// Created by:	Sandeep Kumar(sxkuma5)	2/25/2005                        
//=============================================================================
// Copyright © 2004 Qwest Communications Inc. All rights reserved.
//============================================================================

using System;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Security;
using System.Security.Principal;
using System.Security.Permissions;
using Microsoft.ApplicationBlocks.ExceptionManagement;

namespace CenturyLink.ApplicationBlocks.ExceptionManager
{
	/// <summary>
	/// Summary description for ExceptionManager.
	/// </summary>
    public class ExceptionManager : IExceptionPublisher
	{
        // Member variable declarations
        protected static string publicKeyToken = null;
        private string logName = "Application";
        private const string logType = "LogType";
        private const string EXCEPTION = "EXCEPTION";
        private const string ALARM = "ALARM";
        private string applicationName = "ExceptionManagerPublishedException";
        private const string TEXT_SEPARATOR = "*********************************************";

        public ExceptionManager()
        {
        }

        /// <summary>
        /// Method used to publish exception information and additional information.
        /// </summary>
        /// <param name="exception">The exception object whose information should be published.</param>
        /// <param name="additionalInfo">A collection of additional data that should be published along with the exception information.</param>
        /// <param name="configSettings">A collection of any additional attributes provided in the config settings for the custom publisher.</param>
        public void Publish(Exception exception, NameValueCollection additionalInfo, NameValueCollection configSettings)
        {
            // Create StringBuilder to maintain publishing information.
            StringBuilder strInfo = new StringBuilder();

            #region Record the contents of the AdditionalInfo collection
            // Record the contents of the AdditionalInfo collection.
            if(additionalInfo != null)
            {
                // Record General information.
                strInfo.AppendFormat("{0}General Information {0}{1}{0}Additional Info:", Environment.NewLine, TEXT_SEPARATOR);

                //foreach(string i in additionalInfo)
                for(int i = 0; i < additionalInfo.Count; i++)
                {
                    string key = additionalInfo.GetKey(i);

                    if(logType.Equals(key))
                    {
                        string value = additionalInfo.Get(key);

                        if(EXCEPTION.Equals(value.ToUpper()) || ALARM.Equals(value.ToUpper()))
                            strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, i, additionalInfo.Get(i));
                        else
                            return;
                    }
                }
            }
            #endregion

            // Load Config values if they are provided.
            if(configSettings != null)
            {
                if(configSettings["applicationName"] != null && configSettings["applicationName"].Length > 0) applicationName = configSettings["applicationName"];
                if(configSettings["logName"] != null && configSettings["logName"].Length > 0) logName = configSettings["logName"];
            }

            // Verify that the Source exists before gathering exception information.
            VerifyValidSource();

            if(exception == null)
            {
                strInfo.AppendFormat("{0}{0}No Exception object has been provided.{0}", Environment.NewLine);
            }
            else
            {
                #region Loop through each exception class in the chain of exception objects
                // Loop through each exception class in the chain of exception objects.
                Exception currentException = exception;	// Temp variable to hold InnerException object during the loop.
                int intExceptionCount = 1;				// Count variable to track the number of exceptions in the chain.
                
                do
                {
                    // Write title information for the exception object.
                    strInfo.AppendFormat("{0}{0}{1}) Exception Information{0}{2}", Environment.NewLine, intExceptionCount.ToString(), TEXT_SEPARATOR);
                    strInfo.AppendFormat("{0}Exception Type: {1}", Environment.NewLine, currentException.GetType().FullName);

                    #region Loop through the public properties of the exception object and record their value
                    // Loop through the public properties of the exception object and record their value.
                    PropertyInfo[] aryPublicProperties = currentException.GetType().GetProperties();
                    NameValueCollection currentAdditionalInfo;

                    foreach(PropertyInfo p in aryPublicProperties)
                    {
                        // Do not log information for the InnerException or StackTrace. This information is 
                        // captured later in the process.
                        if(p.Name != "InnerException" && p.Name != "StackTrace")
                        {
                            if(p.GetValue(currentException, null) == null)
                            {
                                strInfo.AppendFormat("{0}{1}: NULL", Environment.NewLine, p.Name);
                            }
                            else
                            {
                                // Loop through the collection of AdditionalInformation if the exception type is a BaseApplicationException.
                                if(p.Name == "AdditionalInformation" && currentException is BaseApplicationException)
                                {
                                    // Verify the collection is not null.
                                    if(p.GetValue(currentException, null) != null)
                                    {
                                        // Cast the collection into a local variable.
                                        currentAdditionalInfo = (NameValueCollection)p.GetValue(currentException, null);

                                        // Check if the collection contains values.
                                        if(currentAdditionalInfo.Count > 0)
                                        {
                                            strInfo.AppendFormat("{0}AdditionalInformation:", Environment.NewLine);

                                            // Loop through the collection adding the information to the string builder.
                                            for(int i = 0; i < currentAdditionalInfo.Count; i++)
                                            {
                                                strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, currentAdditionalInfo.GetKey(i), currentAdditionalInfo[i]);
                                            }
                                        }
                                    }
                                }
                                // Otherwise just write the ToString() value of the property.
                                else
                                {
                                    strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, p.Name, p.GetValue(currentException, null));
                                }
                            }
                        }
                    }
                    #endregion
                    #region Record the Exception StackTrace
                    // Record the StackTrace with separate label.
                    if(currentException.StackTrace != null)
                    {
                        strInfo.AppendFormat("{0}{0}StackTrace Information{0}{1}", Environment.NewLine, TEXT_SEPARATOR);
                        strInfo.AppendFormat("{0}{1}", Environment.NewLine, currentException.StackTrace);
                    }
                    #endregion

                    // Reset the temp exception object and iterate the counter.
                    currentException = currentException.InnerException;
                    intExceptionCount++;
                } while(currentException != null);
                #endregion
            }

            // Write the entry to the event log.

            EventLogEntryType eventType = EventLogEntryType.Error;

            if(additionalInfo != null && additionalInfo["sentry.severity"] != null && additionalInfo["sentry.severity"].Length > 0)
            {
                switch(additionalInfo["sentry.severity"].ToLower())
                {
                    case "minor":
                        eventType = EventLogEntryType.Warning;
                        break;
                    case "warning":
                        eventType = EventLogEntryType.Warning;
                        break;
                    case "normal":
                        eventType = EventLogEntryType.Information;
                        break;
                    case "info":
                        eventType = EventLogEntryType.Information;
                        break;
                    case "debug":
                        eventType = EventLogEntryType.Information;
                        break;
                    case "unknown":
                        eventType = EventLogEntryType.Information;
                        break;
                }
            }

            WriteToLog(strInfo.ToString(), eventType);
        }

        /// <summary>
        /// Method used to publish exception information and additional information.
        /// </summary>
        /// <param name="exception">The exception object whose information should be published.</param>
        /// <param name="additionalInfo">A collection of additional data that should be published along with the exception information.</param>
        /// <param name="configSettings">A collection of any additional attributes provided in the config settings for the custom publisher.</param>
        /// <param name="dummy">Empty string</param>
        public long Publish(Exception exception, NameValueCollection additionalInfo, NameValueCollection configSettings, string dummy)
        {
            long logId = 0;
            // Create StringBuilder to maintain publishing information.
            StringBuilder strInfo = new StringBuilder();

            #region Record the contents of the AdditionalInfo collection
            // Record the contents of the AdditionalInfo collection.
            if (additionalInfo != null)
            {
                // Record General information.
                strInfo.AppendFormat("{0}General Information {0}{1}{0}Additional Info:", Environment.NewLine, TEXT_SEPARATOR);

                //foreach(string i in additionalInfo)
                for (int i = 0; i < additionalInfo.Count; i++)
                {
                    string key = additionalInfo.GetKey(i);

                    if (logType.Equals(key))
                    {
                        string value = additionalInfo.Get(key);

                        if (EXCEPTION.Equals(value.ToUpper()) || ALARM.Equals(value.ToUpper()))
                            strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, i, additionalInfo.Get(i));
                        else
                            return 0;
                    }
                }
            }
            #endregion

            // Load Config values if they are provided.
            if (configSettings != null)
            {
                if (configSettings["applicationName"] != null && configSettings["applicationName"].Length > 0) applicationName = configSettings["applicationName"];
                if (configSettings["logName"] != null && configSettings["logName"].Length > 0) logName = configSettings["logName"];
            }

            // Verify that the Source exists before gathering exception information.
            VerifyValidSource();

            if (exception == null)
            {
                strInfo.AppendFormat("{0}{0}No Exception object has been provided.{0}", Environment.NewLine);
            }
            else
            {
                #region Loop through each exception class in the chain of exception objects
                // Loop through each exception class in the chain of exception objects.
                Exception currentException = exception;	// Temp variable to hold InnerException object during the loop.
                int intExceptionCount = 1;				// Count variable to track the number of exceptions in the chain.

                do
                {
                    // Write title information for the exception object.
                    strInfo.AppendFormat("{0}{0}{1}) Exception Information{0}{2}", Environment.NewLine, intExceptionCount.ToString(), TEXT_SEPARATOR);
                    strInfo.AppendFormat("{0}Exception Type: {1}", Environment.NewLine, currentException.GetType().FullName);

                    #region Loop through the public properties of the exception object and record their value
                    // Loop through the public properties of the exception object and record their value.
                    PropertyInfo[] aryPublicProperties = currentException.GetType().GetProperties();
                    NameValueCollection currentAdditionalInfo;

                    foreach (PropertyInfo p in aryPublicProperties)
                    {
                        // Do not log information for the InnerException or StackTrace. This information is 
                        // captured later in the process.
                        if (p.Name != "InnerException" && p.Name != "StackTrace")
                        {
                            if (p.GetValue(currentException, null) == null)
                            {
                                strInfo.AppendFormat("{0}{1}: NULL", Environment.NewLine, p.Name);
                            }
                            else
                            {
                                // Loop through the collection of AdditionalInformation if the exception type is a BaseApplicationException.
                                if (p.Name == "AdditionalInformation" && currentException is BaseApplicationException)
                                {
                                    // Verify the collection is not null.
                                    if (p.GetValue(currentException, null) != null)
                                    {
                                        // Cast the collection into a local variable.
                                        currentAdditionalInfo = (NameValueCollection)p.GetValue(currentException, null);

                                        // Check if the collection contains values.
                                        if (currentAdditionalInfo.Count > 0)
                                        {
                                            strInfo.AppendFormat("{0}AdditionalInformation:", Environment.NewLine);

                                            // Loop through the collection adding the information to the string builder.
                                            for (int i = 0; i < currentAdditionalInfo.Count; i++)
                                            {
                                                strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, currentAdditionalInfo.GetKey(i), currentAdditionalInfo[i]);
                                            }
                                        }
                                    }
                                }
                                // Otherwise just write the ToString() value of the property.
                                else
                                {
                                    strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, p.Name, p.GetValue(currentException, null));
                                }
                            }
                        }
                    }
                    #endregion
                    #region Record the Exception StackTrace
                    // Record the StackTrace with separate label.
                    if (currentException.StackTrace != null)
                    {
                        strInfo.AppendFormat("{0}{0}StackTrace Information{0}{1}", Environment.NewLine, TEXT_SEPARATOR);
                        strInfo.AppendFormat("{0}{1}", Environment.NewLine, currentException.StackTrace);
                    }
                    #endregion

                    // Reset the temp exception object and iterate the counter.
                    currentException = currentException.InnerException;
                    intExceptionCount++;
                } while (currentException != null);
                #endregion
            }

            // Write the entry to the event log.

            EventLogEntryType eventType = EventLogEntryType.Error;

            if (additionalInfo != null && additionalInfo["sentry.severity"] != null && additionalInfo["sentry.severity"].Length > 0)
            {
                switch (additionalInfo["sentry.severity"].ToLower())
                {
                    case "minor":
                        eventType = EventLogEntryType.Warning;
                        break;
                    case "warning":
                        eventType = EventLogEntryType.Warning;
                        break;
                    case "normal":
                        eventType = EventLogEntryType.Information;
                        break;
                    case "info":
                        eventType = EventLogEntryType.Information;
                        break;
                    case "debug":
                        eventType = EventLogEntryType.Information;
                        break;
                    case "unknown":
                        eventType = EventLogEntryType.Information;
                        break;
                }
            }

            WriteToLog(strInfo.ToString(), eventType);
            return logId;
        }

        /// <summary>
        /// Helper function to write an entry to the Event Log.
        /// </summary>
        /// <param name="entry">The entry to enter into the Event Log.</param>
        /// <param name="type">The EventLogEntryType to be used when the entry is logged to the Event Log.</param>
        private void WriteToLog(string entry, EventLogEntryType type)
        {
            try
            {
                // Write the entry to the Event Log.
                EventLog.WriteEntry(applicationName, entry, type);
            }
            catch(SecurityException e)
            {
                throw new SecurityException(String.Format("The event source {0} does not exist and cannot be created with the current permissions.", applicationName), e);
            }
        }

        private void VerifyValidSource()
        {
            try
            {
                if(!EventLog.SourceExists(applicationName))
                    EventLog.CreateEventSource(applicationName, logName);
            }
            catch(SecurityException e)
            {
                throw new SecurityException(String.Format("The event source {0} does not exist and cannot be created with the current permissions.", applicationName), e);
            }
        }

        //protected static void GetCallingAssemblyInfo(string fullName)
        //{
        //    //if(applicationName == null && publicKeyToken == null)
        //    if(publicKeyToken == null)
        //    {
        //        //int pos = fullName.IndexOf(",");
        //        //applicationName = fullName.Substring(0,pos);
        //        int pos = fullName.IndexOf("PublicKeyToken=");
        //        publicKeyToken = fullName.Substring(pos + 15, fullName.Length - pos - 15);
        //    }
        //}
	}
}
