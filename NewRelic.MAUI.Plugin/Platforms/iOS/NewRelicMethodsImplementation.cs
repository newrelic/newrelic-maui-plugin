/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

using Foundation;
using NRIosAgent = MauiiOS.NewRelic.NewRelic;

namespace NewRelic.MAUI.Plugin;

// All the code in this file is only included on iOS.
public class NewRelicMethodsImplementation : INewRelicMethods
{
    private bool _isUncaughtExceptionHandled;

    private Dictionary<LogLevel, MauiiOS.NewRelic.NRLogLevels> logLevelDict = new Dictionary<LogLevel, MauiiOS.NewRelic.NRLogLevels>()
    {
        { LogLevel.ERROR, MauiiOS.NewRelic.NRLogLevels.Error },
        { LogLevel.WARNING, MauiiOS.NewRelic.NRLogLevels.Warning },
        { LogLevel.INFO, MauiiOS.NewRelic.NRLogLevels.Info },
        { LogLevel.VERBOSE, MauiiOS.NewRelic.NRLogLevels.Verbose },
        { LogLevel.AUDIT, MauiiOS.NewRelic.NRLogLevels.Audit }
    };

    private Dictionary<NetworkFailure, nint> networkFailureDict = new Dictionary<NetworkFailure, nint>()
    {
        { NetworkFailure.Unknown, (nint) MauiiOS.NewRelic.NRNetworkFailureCode.Unknown },
        { NetworkFailure.BadURL, (nint) MauiiOS.NewRelic.NRNetworkFailureCode.BadURL },
        { NetworkFailure.TimedOut, (nint) MauiiOS.NewRelic.NRNetworkFailureCode.TimedOut },
        { NetworkFailure.CannotConnectToHost, (nint) MauiiOS.NewRelic.NRNetworkFailureCode.CannotConnectToHost },
        { NetworkFailure.DNSLookupFailed, (nint) MauiiOS.NewRelic.NRNetworkFailureCode.DNSLookupFailed },
        { NetworkFailure.BadServerResponse, (nint) MauiiOS.NewRelic.NRNetworkFailureCode.BadServerResponse },
        { NetworkFailure.SecureConnectionFailed, (nint) MauiiOS.NewRelic.NRNetworkFailureCode.SecureConnectionFailed }
    };

    private Dictionary<MetricUnit, string> metricUnitDict = new Dictionary<MetricUnit, string>()
    {
        { MetricUnit.PERCENT, "%" },
        { MetricUnit.BYTES, "bytes" },
        { MetricUnit.SECONDS, "sec" },
        { MetricUnit.BYTES_PER_SECOND, "bytes/second" },
        { MetricUnit.OPERATIONS, "op" }
    };

    public void Start(string applicationToken, AgentStartConfiguration agentConfig = null)
    {
        if (agentConfig == null)
        {
            agentConfig = new AgentStartConfiguration();
        }

        NRIosAgent.EnableCrashReporting(agentConfig.crashReportingEnabled);
        NRIosAgent.SetPlatform(MauiiOS.NewRelic.NRMAApplicationPlatform.Maui);
        MauiiOS.NewRelic.NewRelic.SetPlatformVersion("1.1.9");


        MauiiOS.NewRelic.NRLogger.SetLogLevels((uint)logLevelDict[agentConfig.logLevel]);
        if (!agentConfig.loggingEnabled)
        {
            MauiiOS.NewRelic.NRLogger.SetLogLevels((uint)MauiiOS.NewRelic.NRLogLevels.None);
        }

        if (!agentConfig.networkErrorRequestEnabled)
        {
            NRIosAgent.DisableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.RequestErrorEvents);
        }

        if (!agentConfig.networkRequestEnabled)
        {
            NRIosAgent.DisableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.NetworkRequestEvents);
        }

        if (!agentConfig.interactionTracingEnabled)
        {
            NRIosAgent.DisableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.InteractionTracing);
        }

        if (!agentConfig.webViewInstrumentation)
        {
            NRIosAgent.DisableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.WebViewInstrumentation);
        }

        if (agentConfig.fedRampEnabled)
        {
            NRIosAgent.EnableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.FedRampEnabled);
        }
        
        if (agentConfig.offlineStorageEnabled)
        {
            NRIosAgent.EnableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.OfflineStorage);
        }
        else
        {
            NRIosAgent.DisableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.OfflineStorage);
        }
        
        if (agentConfig.newEventSystemEnabled)
        {
            NRIosAgent.EnableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.NewEventSystem);
        }
        else
        {
            NRIosAgent.DisableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.NewEventSystem);
        }

        
        if (agentConfig.backgroundReportingEnabled)
        {
            NRIosAgent.EnableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.BackgroundReporting);
        }
        else
        {
            NRIosAgent.DisableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.BackgroundReporting);
        }




        if (agentConfig.collectorAddress.Equals("DEFAULT") && agentConfig.crashCollectorAddress.Equals("DEFAULT"))
        {
            NRIosAgent.StartWithApplicationToken(applicationToken);
        } else
        {
            string collectorAddress = agentConfig.collectorAddress.Equals("DEFAULT") ?
                "mobile-collector.newrelic.com" : agentConfig.collectorAddress;
            string crashCollectorAddress = agentConfig.crashCollectorAddress.Equals("DEFAULT") ?
                "mobile-crash.newrelic.com" : agentConfig.crashCollectorAddress;
            NRIosAgent.StartWithApplicationToken(applicationToken, collectorAddress, crashCollectorAddress);
        }
    }

    public void CrashNow(string message = "")
    {
        if (string.IsNullOrEmpty(message))
        {
            NRIosAgent.CrashNow();
        }
        else
        {
            NRIosAgent.CrashNow(message);
        }
    }

    public string CurrentSessionId()
    {
        return NRIosAgent.CurrentSessionId;
    }

    public bool RecordBreadcrumb(string name, Dictionary<string, object> attributes)
    {
        return NRIosAgent.RecordBreadcrumb(name, ConvertAttributesToNSDictionary(attributes));
    }

    public void EndInteraction(string interactionId)
    {
        NRIosAgent.StopCurrentInteraction(interactionId);
    }

    public bool IncrementAttribute(string name, float value = 1)
    {
        return NRIosAgent.IncrementAttribute(name, value);
    }

    public void NoticeHttpTransaction(string url, string httpMethod, int statusCode, long startTime, long endTime, long bytesSent, long bytesReceived, string responseBody)
    {
        IntPtr statusCodePtr = new IntPtr(statusCode);

        NRIosAgent.NoticeNetworkRequestForURL(Foundation.NSUrl.FromString(url),
            
            httpMethod,
            startTime,
            endTime,
            new NSDictionary(),
            statusCodePtr,
            (nuint)bytesSent,
            (nuint)bytesReceived,
            new NSData(),
            null,
            null);
        return;
    }

    public void NoticeNetworkFailure(string url, string httpMethod, long startTime, long endTime, NetworkFailure failure)
    {
        NRIosAgent.NoticeNetworkFailureForURL(NSUrl.FromString(url), httpMethod, (double) startTime, (double) endTime, networkFailureDict[failure]);
        return;
    }

    public bool RecordCustomEvent(string eventType, string eventName, Dictionary<string, object> attributes)
    {
        return NRIosAgent.RecordCustomEvent(eventType, eventName, ConvertAttributesToNSDictionary(attributes));
    }

    public void RecordMetric(string name, string category)
    {
        NRIosAgent.RecordMetricWithName(name, category);
        return;
    }

    public void RecordMetric(string name, string category, double value)
    {
        NRIosAgent.RecordMetricWithName(name, category, Foundation.NSNumber.FromDouble(value));
        return;
    }

    public void RecordMetric(string name, string category, double value, MetricUnit countUnit, MetricUnit valueUnit)
    {
        NRIosAgent.RecordMetricWithName(name, category, Foundation.NSNumber.FromDouble(value), metricUnitDict[valueUnit], metricUnitDict[countUnit]);
        return;
    }

    public bool RemoveAllAttributes()
    {
        return NRIosAgent.RemoveAllAttributes;
    }

    public bool RemoveAttribute(string name)
    {
        return NRIosAgent.RemoveAttribute(name);
    }

    public bool SetAttribute(string name, string value)
    {
        return NRIosAgent.SetAttribute(name, Foundation.NSObject.FromObject(value));
    }

    public bool SetAttribute(string name, double value)
    {
        return NRIosAgent.SetAttribute(name, Foundation.NSObject.FromObject(value));
    }

    public bool SetAttribute(string name, bool value)
    {
        return NRIosAgent.SetAttribute(name, Foundation.NSObject.FromObject(value));
    }

    public void SetMaxEventBufferTime(int maxBufferTimeInSec)
    {
        NRIosAgent.SetMaxEventBufferTime((uint)maxBufferTimeInSec);
        return;
    }

    public void SetMaxEventPoolSize(int maxPoolSize)
    {
        NRIosAgent.SetMaxEventPoolSize((uint)maxPoolSize);
        return;
    }
    
    public void SetMaxOfflineStorageSize(int megabytes)
    {
        NRIosAgent.SetMaxOfflineStorageSize((uint)megabytes);
        return;
    }

    public bool SetUserId(string userId)
    {
        return NRIosAgent.SetUserId(userId);
    }

    public string StartInteraction(string interactionName)
    {
        return NRIosAgent.StartInteractionWithName(interactionName);
    }

    public void AnalyticsEventEnabled(bool enabled)
    {
        // This is an Android-only function
        return;
    }

    public void NetworkRequestEnabled(bool enabled)
    {
        if (enabled)
        {
            NRIosAgent.EnableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.NetworkRequestEvents);
        }
        else
        {
            NRIosAgent.DisableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.NetworkRequestEvents);
        }
        return;
    }

    public void NetworkErrorRequestEnabled(bool enabled)
    {
        if (enabled)
        {
            NRIosAgent.EnableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.RequestErrorEvents);
        }
        else
        {
            NRIosAgent.DisableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.RequestErrorEvents);
        }
        return;
    }

    public void HttpResponseBodyCaptureEnabled(bool enabled)
    {
        if (enabled)
        {
            NRIosAgent.EnableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.HttpResponseBodyCapture);
        }
        else
        {
            NRIosAgent.DisableFeatures(MauiiOS.NewRelic.NRMAFeatureFlags.HttpResponseBodyCapture);
        }
        return;
    }

    public void RecordException(Exception exception)
    {
        RecordException(exception, new Dictionary<string, object>());

    }

    public HttpClientHandler GetHttpMessageHandler()
    {
        return new HttpClientHandler(){};
    }

    public void HandleUncaughtException(bool shouldThrowFormattedException = true)
    {
        if (!_isUncaughtExceptionHandled)
        {
            _isUncaughtExceptionHandled = true;
            MauiExceptions.UnhandledException += (s, e) =>
            {

                if (e.ExceptionObject is Exception exception)
                {
                    RecordException(exception);
                }
            };
        }

    }

    public void TrackShellNavigatedEvents()
    {
        Shell.Current.Navigated += (sender, e) =>
        {
            Dictionary<string, object> attr = new Dictionary<string, object>();
            if (e.Previous != null)
            {
                attr.Add("Previous", e.Previous.Location.ToString());
            }
            attr.Add("Current", e.Current.Location.ToString());
            attr.Add("Source", e.Source.ToString());
            this.RecordBreadcrumb("ShellNavigated", attr);
        };
    }

    public void Shutdown()
    {
        NRIosAgent.Shutdown();
        return;
    }

    public void AddHTTPHeadersTrackingFor(List<string> headers)
    {
        NRIosAgent.AddHTTPHeaderTrackingFor(headers.ToArray());
    }

    public List<string> GetHTTPHeadersTrackingFor()
    {
        // return NRIosAgent.HttpHeadersAddedForTracking().ToList();
        return null;
    }
    
        public void LogInfo(String message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            NRIosAgent.LogInfo(message);
        }
        else 
        {
            Console.WriteLine("Info: Message is empty or null.");
        }
    }
  
    public void LogWarning(String message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            NRIosAgent.LogWarning(message);
        }
        else
        {
            Console.WriteLine("Warning: Message is empty or null.");
        }
    }

    public void LogDebug(String message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            NRIosAgent.LogDebug(message);
        }
        else
        {
            Console.WriteLine("Debug: Message is empty or null.");
        }
    }

    public void LogVerbose(String message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            NRIosAgent.LogVerbose(message);
        }
        else
        {
            Console.WriteLine("Verbose: Message is empty or null.");
        }
    }
    
    public void LogError(String message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            NRIosAgent.LogError(message);
        }
        else
        {
            Console.WriteLine("Error: Message is empty or null.");
        }
    }

    public void Log(LogLevel level, String message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            Dictionary<string,object> attributes = new Dictionary<string,object>();
            attributes.Add("Message", message);
            attributes.Add("logLevel", level.ToString());
            
            NRIosAgent.LogAll(ConvertAttributesToNSDictionary(attributes));
        }
        else
        {
            Console.WriteLine($"Log Level {level}: Message is empty or null.");
        }
    }
    
   
    
    public void LogAttributes(Dictionary<string, object> attributes)
    {
        if(attributes != null && attributes.Count > 0) 
        {
            NRIosAgent.LogAttributes(ConvertAttributesToNSDictionary(attributes));
        }
        else
        {
            Console.WriteLine("Attributes are empty or null.");
        }
    }
    
    public Foundation.NSMutableDictionary ConvertAttributesToNSDictionary(Dictionary<string, object> attributes)
    {
        Foundation.NSMutableDictionary NSDict = new Foundation.NSMutableDictionary();
        foreach (KeyValuePair<string, object> entry in attributes)
        {
            NSDict.Add(Foundation.NSObject.FromObject(entry.Key), Foundation.NSObject.FromObject(entry.Value));
        }
        return NSDict;
    }
    

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void RecordException(Exception exception, Dictionary<string, object> attributes)
    {
        List<NativeStackFrame> stackFrames = StackTraceParser.Parse(exception.StackTrace).ToList();
        var _stackFramesArray = new NSMutableArray();
        foreach (NativeStackFrame length in stackFrames)
        {
            var stackFrameKeys = new object[] { "file", "line", "method", "class" };
            var stackFrameObjects = new object[] { length.FileName, length.LineNumber, length.MethodName, length.ClassName };
            NSDictionary dictionary = NSDictionary.FromObjectsAndKeys(stackFrameObjects, stackFrameKeys);
            _stackFramesArray.Add(dictionary);
        }

        var errorKeys = new object[] { "name", "reason", "cause", "fatal", "stackTraceElements", "attributes" };
        var errorObjects = new object[] { exception.Message, exception.Message, exception.Message, false, _stackFramesArray,ConvertAttributesToNSDictionary(attributes)};
        NSDictionary NSDict = NSDictionary.FromObjectsAndKeys(errorObjects, errorKeys);


        NRIosAgent.RecordHandledExceptionWithStackTrace(NSDict);
    }
}

