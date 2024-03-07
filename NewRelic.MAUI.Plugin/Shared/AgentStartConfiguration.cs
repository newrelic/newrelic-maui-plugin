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
        /// Android Specific
        /// Optional:Enable or disable collection of event data.
        /// </summary>
        public bool analyticsEventEnabled = true;
        /// <summary>
		/// Enable or disable reporting network and HTTP request errors to the MobileRequestError event type.
		/// </summary>
        public bool networkErrorRequestEnabled = true;
        /// <summary>
		/// Enable or disable reporting successful HTTP requests to the MobileRequest event type.
		/// </summary>
        public bool networkRequestEnabled = true;
        /// <summary>
        /// iOS Specific
        /// Enable/Disable automatic instrumentation of WebViews.
        /// </summary>
        public bool webViewInstrumentation = true;
        /// <summary>
		/// Enable or disable reporting data using different endpoints for US government clients.
		/// </summary>
        public bool fedRampEnabled = false;
        /// <summary>
		/// Enable or disable interaction tracing. Trace instrumentation still occurs, but no traces are harvested. This will disable default and custom interactions
		/// </summary>
        public bool interactionTracingEnabled = false;
        /// <summary>
        /// Enable or disable offline data storage when no internet connection is available.
        /// </summary>
        public bool offlineStorageEnabled = true;


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
        /// <param name="analyticsEventEnabled">Enable or disable collection of event data.</param>
        /// <param name="networkErrorRequestEnabled">Enable or disable reporting network and HTTP request errors to the MobileRequestError event type.</param>
        /// <param name="networkRequestEnabled">Enable or disable reporting successful HTTP requests to the MobileRequest event type.</param>
        /// <param name="interactionTracingEnabled">Enable or disable interaction tracing. Trace instrumentation still occurs, but no traces are harvested. This will disable default and custom interactions.</param>
        /// <param name="webViewInstrumentation">Enable/Disable automatic instrumentation of WebViews.</param>
        /// <param name="fedRampEnabled">Enable or disable reporting data using different endpoints for US government clients</param>
        /// <param name="offlineStorageEnabled">Enable or disable offline data storage when no internet connection is available.</param>

        
        public AgentStartConfiguration(bool crashReportingEnabled = true, bool loggingEnabled = true, LogLevel logLevel = LogLevel.INFO, string collectorAddress = "DEFAULT",
            string crashCollectorAddress = "DEFAULT", bool analyticsEventEnabled = true, bool networkErrorRequestEnabled = true, bool networkRequestEnabled = true,
            bool interactionTracingEnabled = false, bool webViewInstrumentation = true, bool fedRampEnabled = false, bool offlineStorageEnabled = true)
        {
            this.crashReportingEnabled = crashReportingEnabled;
            this.loggingEnabled = loggingEnabled;
            this.logLevel = logLevel;
            this.collectorAddress = collectorAddress;
            this.crashCollectorAddress = crashCollectorAddress;
            this.analyticsEventEnabled = analyticsEventEnabled;
            this.networkErrorRequestEnabled = networkErrorRequestEnabled;
            this.networkRequestEnabled = networkRequestEnabled;
            this.interactionTracingEnabled = interactionTracingEnabled;
            this.webViewInstrumentation = webViewInstrumentation;
            this.fedRampEnabled = fedRampEnabled;
            this.offlineStorageEnabled = offlineStorageEnabled;
        }
    }
}

