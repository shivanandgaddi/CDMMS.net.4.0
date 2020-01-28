//=============================================================================
// Class  : ExceptionConstants           
// Created by:	Sandeep Kumar(sxkuma5)	2/25/2005                  
//=============================================================================
// Copyright © 2004 Qwest Communications Inc. All rights reserved.
//============================================================================

using System;

namespace CenturyLink.ApplicationBlocks.ExceptionManager
{
	/// <summary>
	/// Summary description for ExceptionConstants.
	/// </summary>
	public class ExceptionConstants
	{
		public  enum  Severity 
		{
			Critical=1,
			Major=2,
			Minor=3,
			Warning=4,
			Normal=5,
			Unknown=6,
		}

		public enum LogTo
		{
			DefaultAndSentryAndDB=0,
			SentryAndDB,
			DefaultAndDB,
			DefaultAndSentry,
		    Sentry,
		    Default,
		    DB,
			None,
			All
		}

		public static Severity defaultSeverity = Severity.Normal;
		public static LogTo defaultLogTo = LogTo.All;
		public static int defaultErrorId = 1; //ErrorId.Unknown;
		public static int informationId = 2; // for pure information publish
		public static string defaultMessage = "No Error Message Specified.";
	}

	
}
