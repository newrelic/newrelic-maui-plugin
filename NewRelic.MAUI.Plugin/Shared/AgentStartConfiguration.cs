/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

using System;
namespace NewRelic.MAUI.Plugin
{
	/// <summary>
	/// Specifies the log level emitted by the agent.
	/// </summary>
	public enum LogLevel
	{
		ERROR,
		WARNING,
		INFO,
		VERBOSE,
		AUDIT
	}

	public class AgentStartConfiguration
	{
		/// <summary>
		/// Enable or disable crash reporting.
		/// </summary>
		public bool crashReportingEnabled = true;
		/// <summary>
		/// Enable or disable agent logging.
		/// </summary>
		public bool loggingEnabled = true;
		/// <summary>
		/// Specifies the log level.
		/// </summary>
		public LogLevel logLevel = LogLevel.INFO;
		/// <summary>
		/// Specifies the URI authority component of the harvest data upload endpoint.
		/// </summary>
		public string collectorAddress = "DEFAULT";
		/// <summary>
		/// Specifies the authority component of the crash data upload URI.
		/// </summary>
		public string crashCollectorAddress = "DEFAULT";

		/// <summary>
		/// Initialize the AgentStartConfiguration with default settings.
		/// </summary>
		public AgentStartConfiguration()
		{
		}

		/// <summary>
		/// Initialize the AgentStartConfiguration with custom settings.
		/// </summary>
		/// <param name="crashReportingEnabled">Enable or disable crash reporting.</param>
		/// <param name="loggingEnabled">Enable or disable agent logging.</param>
		/// <param name="logLevel">Specifies the log level.</param>
		/// <param name="collectorAddress">Specifies the URI authority component of the harvest data upload endpoint.</param>
		/// <param name="crashCollectorAddress">Specifies the authority component of the crash data upload URI.</param>
		public AgentStartConfiguration(bool crashReportingEnabled, bool loggingEnabled, LogLevel logLevel, string collectorAddress, string crashCollectorAddress)
		{
			this.crashReportingEnabled = crashReportingEnabled;
			this.loggingEnabled = loggingEnabled;
			this.logLevel = logLevel;
			this.collectorAddress = collectorAddress;
			this.crashCollectorAddress = crashCollectorAddress;
		}
	}
}

