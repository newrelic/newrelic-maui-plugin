/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

namespace NewRelic.MAUI.Plugin
{
    /// <summary>  
    /// Unit of measurement for the metric count, including PERCENT, BYTES, SECONDS, BYTES_PER_SECOND, or OPERATIONS
    /// </summary>  
    public enum MetricUnit
    {
        PERCENT,
        BYTES,
        SECONDS,
        BYTES_PER_SECOND,
        OPERATIONS
    }

    /// <summary>
    /// The type of network failure that occurred.
    /// If an exception cannot be resolved to a network failure automatically, this enum can be used to categorize the failure accurately.
    /// </summary>
    public enum NetworkFailure
    {
        Unknown,
        BadURL,
        TimedOut,
        CannotConnectToHost,
        DNSLookupFailed,
        BadServerResponse,
        SecureConnectionFailed
    }

	public interface INewRelicMethods : IDisposable
    {
        /// <summary>
        /// Starts the agent. It is recommended to use separate tokens for Android and iOS.
        /// If needed, use AgentStartConfiguration to customize the agent on start.
        /// </summary>
        /// <param name="applicationToken"></param>
        /// <param name="agentConfig"></param>
        void Start(string applicationToken, AgentStartConfiguration agentConfig = null);

        /// <summary>
        /// Throws a demo run-time exception on Android/iOS to test New Relic crash reporting.
        /// </summary>
        /// <param name="message">A message attached to the exception</param>
        void CrashNow(string message = "");

        /// <summary>
        /// Returns ID for the current session.
        /// </summary>
        /// <returns>Returns ID string for the current session.</returns>
        string CurrentSessionId();

        /// <summary>
        /// Track a method as an interaction.
        /// </summary>
        /// <param name="interactionName">The name you want to give to the interaction</param>
        /// <returns>Returns an interaction ID number which can be used for ending the interaction at a certain point.</returns>
        string StartInteraction(string interactionName);

        /// <summary>
        /// End an interaction
        /// </summary>
        /// <param name="interactionId">The string ID for the interaction you want to end. This string is returned when you use startInteraction().</param>
        void EndInteraction(string interactionId);

        /// <summary>
        /// Tracks network requests manually. You can use this method to record HTTP transactions, with an option to also send a response body.
        /// </summary>
        /// <param name="url">URL of the request.</param>
        /// <param name="httpMethod">HTTP method used, such as GET or POST.</param>
        /// <param name="statusCode">Status code of the HTTP response, such as 200 for OK.</param>
        /// <param name="startTime">Start time of the request in milliseconds since the epoch.</param>
        /// <param name="endTime">End time of the request in milliseconds since the epoch.</param>
        /// <param name="bytesSent">The number of bytes sent in the request.</param>
        /// <param name="bytesReceived">The number of bytes received in the response.</param>
        /// <param name="responseBody">The response body of the HTTP response.</param>
        void NoticeHttpTransaction(string url,
            string httpMethod,
            int statusCode,
            long startTime,
            long endTime,
            long bytesSent,
            long bytesReceived,
            string responseBody);

        /// <summary>
        /// Records network failures.
        /// </summary>
        /// <param name="url">URL of the request.</param>
        /// <param name="httpMethod">HTTP method used, such as GET or POST.</param>
        /// <param name="startTime">Start time of the request in milliseconds since the epoch.</param>
        /// <param name="endTime">End time of the request in milliseconds since the epoch.</param>
        /// <param name="failure">The type of network failure that occurred.</param>
        void NoticeNetworkFailure(string url, string httpMethod, long startTime, long endTime, NetworkFailure failure);


        /// <summary>
        /// This call creates and records a MobileBreadcrumb event, which can be queried with NRQL and in the crash event trail.
        /// Mobile breadcrumbs are useful for crash analysis; they should be created for app activity that may be helpful for troubleshooting crashes.
        /// </summary>
        /// <param name="name">The name you want to give to the breadcrumb event.</param>
        /// <param name="attributes">A dictionary that includes a list of attributes of the breadcrumb event.</param>
        /// <returns>Returns true if the event is recorded successfully, or false if not.</returns>
        bool RecordBreadcrumb(string name, Dictionary<string, object> attributes);

        /// <summary>
        /// Creates and records a custom event for use in New Relic Insights.
        /// </summary>
        /// <param name="eventType">The type of event. Do not use eventType to name your custom events. Instead, use a custom attribute or the name.</param>
        /// <param name="eventName">Name the event.</param>
        /// <param name="attributes">A dictionary that includes a list of attributes that further designate subcategories to the eventType.</param>
        /// <returns>Returns true if the event is recorded successfully, or false if not.</returns>
        bool RecordCustomEvent(string eventType, string eventName, Dictionary<string, object> attributes);

        /// <summary>
        /// Record custom metrics (arbitrary numerical data).
        /// </summary>
        /// <param name="name">The desired name for the custom metric.</param>
        /// <param name="category">The metric category name, either custom or using a predefined metric category.</param>
        void RecordMetric(string name, string category);

        /// <summary>
        /// Record custom metrics (arbitrary numerical data).
        /// </summary>
        /// <param name="name">The desired name for the custom metric.</param>
        /// <param name="category">The metric category name, either custom or using a predefined metric category.</param>
        /// <param name="value">The value of the metric.</param>
        void RecordMetric(string name, string category, double value);

        /// <summary>
        /// Record custom metrics (arbitrary numerical data).
        /// </summary>
        /// <param name="name">The desired name for the custom metric.</param>
        /// <param name="category">The metric category name, either custom or using a predefined metric category.</param>
        /// <param name="value">The value of the metric.</param>
        /// <param name="countUnit">Unit of measurement for the metric count.</param>
        /// <param name="valueUnit">Unit of measurement for the metric value.</param>
        void RecordMetric(string name, string category, double value, MetricUnit countUnit, MetricUnit valueUnit);

        /// <summary>
        /// Creates a session-level attribute shared by multiple mobile event types. Overwrites its previous value and type each time it is called.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="value">String value of the attribute.</param>
        /// <returns>Returns true if the event is recorded successfully, or false if not.</returns>
        bool SetAttribute(string name, string value);

        /// <summary>
        /// Creates a session-level attribute shared by multiple mobile event types. Overwrites its previous value and type each time it is called.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="value">Double value of the attribute.</param>
        /// <returns>Returns true if the event is recorded successfully, or false if not.</returns>
        bool SetAttribute(string name, double value);

        /// <summary>
        /// Creates a session-level attribute shared by multiple mobile event types. Overwrites its previous value and type each time it is called.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="value">Boolean value of the attribute.</param>
        /// <returns>Returns true if the event is recorded successfully, or false if not.</returns>
        bool SetAttribute(string name, bool value);

        /// <summary>
        /// Increments the count of an attribute. Overwrites its previous value and type each time it is called.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">Optional. The attribute is incremented by this float value.</param>
        /// <returns>Returns true if recorded successfully, or false if not.</returns>
        bool IncrementAttribute(string name, float value = 1);

        /// <summary>
        /// Removes an attribute
        /// </summary>
        /// <param name="name">The name of the attribute that you want to remove.</param>
        /// <returns>Returns true if it succeeds, or false if it doesn't.</returns>
        bool RemoveAttribute(string name);

        /// <summary>
        /// Removes all attributes from the session.
        /// </summary>
        /// <returns>Returns true if it succeeds, or false if it doesn't.</returns>
        bool RemoveAllAttributes();

        /// <summary>
        /// Sets the event harvest cycle length.
        /// </summary>
        /// <param name="maxBufferTimeInSec">The maximum time (in seconds) that the agent should store events in memory. The default value harvest cycle length is 600 seconds.</param>
        void SetMaxEventBufferTime(int maxBufferTimeInSec);

        /// <summary>
        /// Sets the maximum size of the event pool.
        /// </summary>
        /// <param name="maxPoolSize">Maximum size of event pool.</param>
        void SetMaxEventPoolSize(int maxPoolSize);

        /// <summary>
        /// Set a custom user identifier value to associate user sessions with analytics events and attributes.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Returns true if it succeeds, or false if it doesn't.</returns>
        bool SetUserId(string userId);

        /// <summary>
        /// FOR ANDROID ONLY. Enable or disable collection of event data.
        /// </summary>
        /// <param name="enabled"></param>
        void AnalyticsEventEnabled(bool enabled);

        /// <summary>
        /// Enable or disable reporting successful HTTP requests to the MobileRequest event type.
        /// </summary>
        /// <param name="enabled"></param>
        void NetworkRequestEnabled(bool enabled);

        /// <summary>
        /// Enable or disable reporting network and HTTP request errors to the MobileRequestError event type.
        /// </summary>
        /// <param name="enabled"></param>
        void NetworkErrorRequestEnabled(bool enabled);

        /// <summary>
        /// Enable or disable capture of HTTP response bodies for HTTP error traces, and MobileRequestError events.
        /// </summary>
        /// <param name="enabled"></param>
        void HttpResponseBodyCaptureEnabled(bool enabled);

        /// <summary>
        /// Records a handled exception.
        /// </summary>
        /// <param name="exception">The exception object that was thrown.</param>
        void RecordException(System.Exception exception);

        /// <summary>
        /// Records a handled exception with Attributes.
        /// </summary>
        /// <param name="exception">The exception object that was thrown.</param>
        /// <param name="attributes">A dictionary that includes a list of attributes that further designate subcategories to the Exception.</param>
        void RecordException(System.Exception exception, Dictionary<string, object> attributes);

        /// <summary>
        /// Provides a HttpMessageHandler to instrument http requests through HttpClient.
        /// </summary>
        /// <returns>The HttpMessageHandler object to be used in HttpClient.</returns>
        HttpClientHandler GetHttpMessageHandler();

        /// <summary>
        /// Records unhandled exceptions to New Relic. It is recommended to initialize the handler prior to starting the agent.
        /// </summary>
        /// <param name="shouldThrowFormattedException">Flag for formatting exceptions</param>
        void HandleUncaughtException(bool shouldThrowFormattedException = true);

        /// <summary>
        /// Track navigation events within the .NET MAUI Shell. Records a breadcrumb named "ShellNavigated" with the `Navigated` event properties. 
        /// </summary>
        void TrackShellNavigatedEvents();

        /// <summary>
        /// Shut down the agent within the current application lifecycle during runtime.
        /// </summary>
        void Shutdown();

        void AddHTTPHeadersTrackingFor(List<String> headers);

        List<String> GetHTTPHeadersTrackingFor();
        
        /// <summary>
        /// Sets the maximum size of total data that can be stored for offline storage.
        /// </summary>
        /// <param name="megabytes"> Maximum size of total data that can be stored for offline storage.</param>
        void SetMaxOfflineStorageSize(int megabytes);
        
        
        /// <summary>
        /// To log informational messages. It takes a string message as an argument. 
        /// </summary>
        void LogInfo(String message);
        
        /// <summary>
        /// To log Error messages. It takes a string message as an argument. 
        /// </summary>
        void LogError(String message);
        
        /// <summary>
        /// To log verbose messages, which are typically detailed and diagnostic in nature. It takes a string message as an argument. 
        /// </summary>
        void LogVerbose(String message);
        
        /// <summary>
        /// To log warning messages, which are typically detailed and diagnostic in nature. It takes a string message as an argument. 
        /// </summary>
        void LogWarning(String message);
        
        /// <summary>
        /// To log debug messages, which are usually used for debugging purposes. It takes a string message as an argument. 
        /// </summary>
        void LogDebug(String message);

        /// <summary>
        /// To Log messages with a specific log level. It takes a log level and a string message as arguments.
        /// </summary>
        void Log(LogLevel level, String message);

        /// <summary>
        /// To  log a set of attributes. It takes a dictionary of string keys and object values as an argument.
        /// </summary>
        void LogAttributes(Dictionary<string, object> attributes);
        
    }
}

