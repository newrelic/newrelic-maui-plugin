using System;
namespace NewRelic.MAUI.Plugin
{
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
		public bool crashReportingEnabled = true;
		public bool loggingEnabled = true;
		public LogLevel logLevel = LogLevel.INFO;
		public string collectorAddress = "DEFAULT";
		public string crashCollectorAddress = "DEFAULT";
		
		public AgentStartConfiguration()
		{
		}

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

