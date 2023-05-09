/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

using Foundation;
using plugin.NRTest;
using System.Diagnostics;
using NRIosAgent = iOS.NewRelic.NewRelic;

namespace Plugin.NRTest;

// All the code in this file is only included on iOS.
public class NewRelicMethodsImplementation:INewRelicMethods
{
    private bool _isUncaughtExceptionHandled;

    public void Start(string applicationToken)
    {

        NRIosAgent.SetPlatform(iOS.NewRelic.NRMAApplicationPlatform.Xamarin);
        NRIosAgent.EnableCrashReporting(false);
        iOS.NewRelic.NRLogger.SetLogLevels((uint)iOS.NewRelic.NRLogLevels.All);
        NRIosAgent.StartWithApplicationToken(applicationToken);
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
            null,
            statusCode,
            (nuint)bytesSent,
            (nuint)bytesReceived,
            null,
            null,
            null);
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

    //public void RecordMetric(string name, string category, double value, NewRelicXamarin.MetricUnit countUnit, NewRelicXamarin.MetricUnit valueUnit)
    //{
    //    //NRIosAgent.RecordMetricWithName(name, category, value, metricUnitsDict[valueUnit], metricUnitsDict[countUnit]);
    //    //return;
    //}

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

    public void HandleUncaughtException(bool shouldThrowFormattedException = true)
    {
        if (!_isUncaughtExceptionHandled)
        {
            _isUncaughtExceptionHandled = true;
            Console.WriteLine(_isUncaughtExceptionHandled);
            MauiExceptions.UnhandledException += (s, e) =>
            {
                Console.WriteLine("in ios Error");

                if (e.ExceptionObject is Exception exception)
                {
                    RecordException(exception);
                }
            };
        }

    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public HttpMessageHandler GetHttpMessageHandler()
    {
        throw new NotImplementedException();
    }
}

