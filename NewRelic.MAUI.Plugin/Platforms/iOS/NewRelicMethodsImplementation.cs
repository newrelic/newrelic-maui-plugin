/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

using Foundation;
using System.Diagnostics;
using NRIosAgent = iOS.NewRelic.NewRelic;

namespace NewRelic.MAUI.Plugin;

// All the code in this file is only included on iOS.
public class NewRelicMethodsImplementation : INewRelicMethods
{
    private bool _isUncaughtExceptionHandled;

    private Dictionary<LogLevel, iOS.NewRelic.NRLogLevels> logLevelDict = new Dictionary<LogLevel, iOS.NewRelic.NRLogLevels>()
    {
        { LogLevel.ERROR, iOS.NewRelic.NRLogLevels.Error },
        { LogLevel.WARNING, iOS.NewRelic.NRLogLevels.Warning },
        { LogLevel.INFO, iOS.NewRelic.NRLogLevels.Info },
        { LogLevel.VERBOSE, iOS.NewRelic.NRLogLevels.Verbose },
        { LogLevel.AUDIT, iOS.NewRelic.NRLogLevels.Audit }
    };

    private Dictionary<NetworkFailure, nint> networkFailureDict = new Dictionary<NetworkFailure, nint>()
    {
        { NetworkFailure.Unknown, (nint) iOS.NewRelic.NRNetworkFailureCode.Unknown },
        { NetworkFailure.BadURL, (nint) iOS.NewRelic.NRNetworkFailureCode.BadURL },
        { NetworkFailure.TimedOut, (nint) iOS.NewRelic.NRNetworkFailureCode.TimedOut },
        { NetworkFailure.CannotConnectToHost, (nint) iOS.NewRelic.NRNetworkFailureCode.CannotConnectToHost },
        { NetworkFailure.DNSLookupFailed, (nint) iOS.NewRelic.NRNetworkFailureCode.DNSLookupFailed },
        { NetworkFailure.BadServerResponse, (nint) iOS.NewRelic.NRNetworkFailureCode.BadServerResponse },
        { NetworkFailure.SecureConnectionFailed, (nint) iOS.NewRelic.NRNetworkFailureCode.SecureConnectionFailed }
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
        NRIosAgent.SetPlatform(iOS.NewRelic.NRMAApplicationPlatform.MAUI);
        iOS.NewRelic.NewRelic.SetPlatformVersion("1.0.0");

        iOS.NewRelic.NRLogger.SetLogLevels((uint)logLevelDict[agentConfig.logLevel]);
        if (!agentConfig.loggingEnabled)
        {
            iOS.NewRelic.NRLogger.SetLogLevels((uint)iOS.NewRelic.NRLogLevels.None);
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

        Foundation.NSMutableDictionary NSDict = new Foundation.NSMutableDictionary();
        foreach (KeyValuePair<string, object> entry in attributes)
        {
            NSDict.Add(Foundation.NSObject.FromObject(entry.Key), Foundation.NSObject.FromObject(entry.Value));
        }

        return NRIosAgent.RecordBreadcrumb(name, NSDict);

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
        NRIosAgent.NoticeNetworkRequestForURL(Foundation.NSUrl.FromString(url),
            httpMethod,
            startTime,
            endTime,
            new NSDictionary(),
            statusCode,
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
        Foundation.NSMutableDictionary NSDict = new Foundation.NSMutableDictionary();
        foreach (KeyValuePair<string, object> entry in attributes)
        {
            NSDict.Add(Foundation.NSObject.FromObject(entry.Key), Foundation.NSObject.FromObject(entry.Value));
        }
        return NRIosAgent.RecordCustomEvent(eventType, eventName, NSDict);
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
            NRIosAgent.EnableFeatures(iOS.NewRelic.NRMAFeatureFlags.NetworkRequestEvents);
        }
        else
        {
            NRIosAgent.DisableFeatures(iOS.NewRelic.NRMAFeatureFlags.NetworkRequestEvents);
        }
        return;
    }

    public void NetworkErrorRequestEnabled(bool enabled)
    {
        if (enabled)
        {
            NRIosAgent.EnableFeatures(iOS.NewRelic.NRMAFeatureFlags.RequestErrorEvents);
        }
        else
        {
            NRIosAgent.DisableFeatures(iOS.NewRelic.NRMAFeatureFlags.RequestErrorEvents);
        }
        return;
    }

    public void HttpResponseBodyCaptureEnabled(bool enabled)
    {
        if (enabled)
        {
            NRIosAgent.EnableFeatures(iOS.NewRelic.NRMAFeatureFlags.HttpResponseBodyCapture);
        }
        else
        {
            NRIosAgent.DisableFeatures(iOS.NewRelic.NRMAFeatureFlags.HttpResponseBodyCapture);
        }
        return;
    }

    public void RecordException(Exception exception)
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

        var errorKeys = new object[] { "name", "reason", "cause", "fatal", "stackTraceElements" };
        var errorObjects = new object[] { exception.Message, exception.Message, exception.Message, false, _stackFramesArray };
        NSDictionary NSDict = NSDictionary.FromObjectsAndKeys(errorObjects, errorKeys);


        NRIosAgent.RecordHandledExceptionWithStackTrace(NSDict);
    }

    public HttpMessageHandler GetHttpMessageHandler()
    {
        return new HttpClientHandler();
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

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

