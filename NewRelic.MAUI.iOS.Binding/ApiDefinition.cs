/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

//namespace iOS.NewRelic
//{

//    // The first step to creating a binding is to add your native framework ("MyLibrary.xcframework")
//    // to the project.
//    // Open your binding csproj and add a section like this
//    // <ItemGroup>
//    //   <NativeReference Include="MyLibrary.xcframework">
//    //     <Kind>Framework</Kind>
//    //     <Frameworks></Frameworks>
//    //   </NativeReference>
//    // </ItemGroup>
//    //
//    // Once you've added it, you will need to customize it for your specific library:
//    //  - Change the Include to the correct path/name of your library
//    //  - Change Kind to Static (.a) or Framework (.framework/.xcframework) based upon the library kind and extension.
//    //    - Dynamic (.dylib) is a third option but rarely if ever valid, and only on macOS and Mac Catalyst
//    //  - If your library depends on other frameworks, add them inside <Frameworks></Frameworks>
//    // Example:
//    // <NativeReference Include="libs\MyTestFramework.xcframework">
//    //   <Kind>Framework</Kind>
//    //   <Frameworks>CoreLocation ModelIO</Frameworks>
//    // </NativeReference>
//    // 
//    // Once you've done that, you're ready to move on to binding the API...
//    //
//    // Here is where you'd define your API definition for the native Objective-C library.
//    //
//    // For example, to bind the following Objective-C class:
//    //
//    //     @interface Widget : NSObject {
//    //     }
//    //
//    // The C# binding would look like this:
//    //
//    //     [BaseType (typeof (NSObject))]
//    //     interface Widget {
//    //     }
//    //
//    // To bind Objective-C properties, such as:
//    //
//    //     @property (nonatomic, readwrite, assign) CGPoint center;
//    //
//    // You would add a property definition in the C# interface like so:
//    //
//    //     [Export ("center")]
//    //     CGPoint Center { get; set; }
//    //
//    // To bind an Objective-C method, such as:
//    //
//    //     -(void) doSomething:(NSObject *)object atIndex:(NSInteger)index;
//    //
//    // You would add a method definition to the C# interface like so:
//    //
//    //     [Export ("doSomething:atIndex:")]
//    //     void DoSomething (NSObject object, nint index);
//    //
//    // Objective-C "constructors" such as:
//    //
//    //     -(id)initWithElmo:(ElmoMuppet *)elmo;
//    //
//    // Can be bound as:
//    //
//    //     [Export ("initWithElmo:")]
//    //     NativeHandle Constructor (ElmoMuppet elmo);
//    //
//    // For more information, see https://aka.ms/ios-binding
//    //

//}

using System;
using Foundation;
using ObjCRuntime;
#if !NET
using NativeHandle = System.IntPtr;
#endif
namespace iOS.NewRelic
{


    // @interface NRTimer : NSObject
    [BaseType(typeof(NSObject))]
    [Protocol]
    interface NRTimer
    {
        // @property (readonly, nonatomic) double startTimeMillis;
        [Export("startTimeMillis")]
        double StartTimeMillis { get; }

        // @property (readonly, nonatomic) double endTimeMillis;
        [Export("endTimeMillis")]
        double EndTimeMillis { get; }

        // -(id)initWithStartTime:(double)startTime andEndTime:(double)endTime;
        [Export("initWithStartTime:andEndTime:")]
        NativeHandle Constructor(double startTime, double endTime);

        // -(double)startTimeInMillis;
        [Export("startTimeInMillis")]
        double StartTimeInMillis { get; }

        // -(double)endTimeInMillis;
        [Export("endTimeInMillis")]
        double EndTimeInMillis { get; }

        // -(void)restartTimer;
        [Export("restartTimer")]
        void RestartTimer();

        // -(void)stopTimer;
        [Export("stopTimer")]
        void StopTimer();

        // -(BOOL)hasRunAndFinished;
        [Export("hasRunAndFinished")]
        bool HasRunAndFinished { get; }

        // -(double)timeElapsedInSeconds;
        [Export("timeElapsedInSeconds")]
        double TimeElapsedInSeconds { get; }

        // -(double)timeElapsedInMilliSeconds;
        [Export("timeElapsedInMilliSeconds")]
        double TimeElapsedInMilliSeconds { get; }
    }

    // @interface NRLogger : NSObject
    [BaseType(typeof(NSObject))]
    [Protocol]
    interface NRLogger
    {
        // +(void)log:(unsigned int)level inFile:(NSString *)file atLine:(unsigned int)line inMethod:(NSString *)method withMessage:(NSString *)message;
        [Static]
        [Export("log:inFile:atLine:inMethod:withMessage:")]
        void Log(uint level, string file, uint line, string method, string message);

        // +(void)setLogLevels:(unsigned int)levels;
        [Static]
        [Export("setLogLevels:")]
        void SetLogLevels(uint levels);

        // +(void)setLogTargets:(unsigned int)targets;
        [Static]
        [Export("setLogTargets:")]
        void SetLogTargets(uint targets);

        // +(NSString *)logFilePath;
        [Static]
        [Export("logFilePath")]
        string LogFilePath { get; }

        // +(void)clearLog;
        [Static]
        [Export("clearLog")]
        void ClearLog();

        // +(NRLogLevels)logLevels;
        [Static]
        [Export("logLevels")]
        NRLogLevels LogLevels { get; }
        
        // +(void)setLogIngestKey:(NSString *)key;
        [Static]
        [Export ("setLogIngestKey:")]
        void SetLogIngestKey (string key);

        // +(void)setLogURL:(NSString *)url;
        [Static]
        [Export ("setLogURL:")]
        void SetLogURL (string url);
        
        // +(void)upload;
        [Static]
        [Export ("upload")]
        void Upload ();
    }

    // @protocol NewRelicCustomInteractionInterface
    /*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/

    [Protocol]
    interface NewRelicCustomInteractionInterface
    {
        // @required -(NSString *)customNewRelicInteractionName;
        [Abstract]
        [Export("customNewRelicInteractionName")]
        string CustomNewRelicInteractionName { get; }
    }

    // @interface NewRelic : NSObject
    [BaseType(typeof(NSObject))]
    [Protocol]
    interface NewRelic
    {
        // +(void)crashNow:(NSString * _Nullable)message;
        [Static]
        [Export("crashNow:")]
        void CrashNow([NullAllowed] string message);

        // +(void)crashNow;
        [Static]
        [Export("crashNow")]
        void CrashNow();

        // +(void)enableFeatures:(NRMAFeatureFlags)featureFlags;
        [Static]
        [Export("enableFeatures:")]
        void EnableFeatures(NRMAFeatureFlags featureFlags);

        // +(void)disableFeatures:(NRMAFeatureFlags)featureFlags;
        [Static]
        [Export("disableFeatures:")]
        void DisableFeatures(NRMAFeatureFlags featureFlags);

        // +(void)enableCrashReporting:(BOOL)enabled;
        [Static]
        [Export("enableCrashReporting:")]
        void EnableCrashReporting(bool enabled);

        // +(void)setApplicationVersion:(NSString * _Nonnull)versionString;
        [Static]
        [Export("setApplicationVersion:")]
        void SetApplicationVersion(string versionString);

        // +(void)setApplicationBuild:(NSString * _Nonnull)buildNumber;
        [Static]
        [Export("setApplicationBuild:")]
        void SetApplicationBuild(string buildNumber);

        // +(void)setPlatform:(NRMAApplicationPlatform)platform;
        [Static]
        [Export("setPlatform:")]
        void SetPlatform(NRMAApplicationPlatform platform);

        // +(void)setPlatformVersion:(NSString * _Nonnull)platformVersion;
        [Static]
        [Export("setPlatformVersion:")]
        void SetPlatformVersion(string platformVersion);

        // +(NSString * _Null_unspecified)currentSessionId;
        [Static]
        [Export("currentSessionId")]
        string CurrentSessionId { get; }

        // +(NSString * _Nullable)crossProcessId;
        [Static]
        [NullAllowed, Export("crossProcessId")]
        string CrossProcessId { get; }

        // +(void)startWithApplicationToken:(NSString * _Nonnull)appToken;
        [Static]
        [Export("startWithApplicationToken:")]
        void StartWithApplicationToken(string appToken);

        // +(void)startWithApplicationToken:(NSString * _Nonnull)appToken andCollectorAddress:(NSString * _Nonnull)url andCrashCollectorAddress:(NSString * _Nonnull)crashCollectorUrl;
        [Static]
        [Export("startWithApplicationToken:andCollectorAddress:andCrashCollectorAddress:")]
        void StartWithApplicationToken(string appToken, string url, string crashCollectorUrl);

        // +(void)startWithApplicationToken:(NSString * _Nonnull)appToken withoutSecurity:(BOOL)disableSSL __attribute__((deprecated("")));
        [Static]
        [Export("startWithApplicationToken:withoutSecurity:")]
        void StartWithApplicationToken(string appToken, bool disableSSL);

        // +(NRTimer * _Null_unspecified)createAndStartTimer;
        [Static]
        [Export("createAndStartTimer")]
        NRTimer CreateAndStartTimer { get; }

        // +(NSString * _Null_unspecified)startInteractionWithName:(NSString * _Null_unspecified)interactionName;
        [Static]
        [Export("startInteractionWithName:")]
        string StartInteractionWithName(string interactionName);

        // +(void)stopCurrentInteraction:(NSString * _Null_unspecified)interactionIdentifier;
        [Static]
        [Export("stopCurrentInteraction:")]
        void StopCurrentInteraction(string interactionIdentifier);

        // +(void)startTracingMethod:(SEL _Null_unspecified)selector object:(id _Null_unspecified)object timer:(NRTimer * _Null_unspecified)timer category:(enum NRTraceType)category;
        [Static]
        [Export("startTracingMethod:object:timer:category:")]
        void StartTracingMethod(Selector selector, NSObject @object, NRTimer timer, NRTraceType category);

        // +(void)endTracingMethodWithTimer:(NRTimer * _Null_unspecified)timer;
        [Static]
        [Export("endTracingMethodWithTimer:")]
        void EndTracingMethodWithTimer(NRTimer timer);

        // +(void)recordMetricWithName:(NSString * _Nonnull)name category:(NSString * _Nonnull)category;
        [Static]
        [Export("recordMetricWithName:category:")]
        void RecordMetricWithName(string name, string category);

        // +(void)recordMetricWithName:(NSString * _Nonnull)name category:(NSString * _Nonnull)category value:(NSNumber * _Nonnull)value;
        [Static]
        [Export("recordMetricWithName:category:value:")]
        void RecordMetricWithName(string name, string category, NSNumber value);

        // +(void)recordMetricWithName:(NSString * _Nonnull)name category:(NSString * _Nonnull)category value:(NSNumber * _Nonnull)value valueUnits:(NRMetricUnit * _Nullable)valueUnits;
        [Static]
        [Export("recordMetricWithName:category:value:valueUnits:")]
        void RecordMetricWithName(string name, string category, NSNumber value, [NullAllowed] string valueUnits);

        // +(void)recordMetricWithName:(NSString * _Nonnull)name category:(NSString * _Nonnull)category value:(NSNumber * _Nonnull)value valueUnits:(NRMetricUnit * _Nullable)valueUnits countUnits:(NRMetricUnit * _Nullable)countUnits;
        [Static]
        [Export("recordMetricWithName:category:value:valueUnits:countUnits:")]
        void RecordMetricWithName(string name, string category, NSNumber value, [NullAllowed] string valueUnits, [NullAllowed] string countUnits);

        // +(void)setURLRegexRules:(NSDictionary<NSString *,NSString *> * _Nonnull)regexRules;
        [Static]
        [Export("setURLRegexRules:")]
        void SetURLRegexRules(NSDictionary<NSString, NSString> regexRules);

        // +(void)noticeNetworkRequestForURL:(NSURL * _Null_unspecified)url httpMethod:(NSString * _Null_unspecified)httpMethod withTimer:(NRTimer * _Null_unspecified)timer responseHeaders:(NSDictionary * _Null_unspecified)headers statusCode:(NSInteger)httpStatusCode bytesSent:(NSUInteger)bytesSent bytesReceived:(NSUInteger)bytesReceived responseData:(NSData * _Null_unspecified)responseData traceHeaders:(NSDictionary<NSString *,NSString *> * _Nullable)traceHeaders andParams:(NSDictionary * _Nullable)params;
        [Static]
        [Export("noticeNetworkRequestForURL:httpMethod:withTimer:responseHeaders:statusCode:bytesSent:bytesReceived:responseData:traceHeaders:andParams:")]
        void NoticeNetworkRequestForURL(NSUrl url, string httpMethod, NRTimer timer, NSDictionary headers, nint httpStatusCode, nuint bytesSent, nuint bytesReceived, NSData responseData, [NullAllowed] NSDictionary<NSString, NSString> traceHeaders, [NullAllowed] NSDictionary @params);

        // +(void)noticeNetworkRequestForURL:(NSURL * _Null_unspecified)url httpMethod:(NSString * _Null_unspecified)httpMethod startTime:(double)startTime endTime:(double)endTime responseHeaders:(NSDictionary * _Null_unspecified)headers statusCode:(NSInteger)httpStatusCode bytesSent:(NSUInteger)bytesSent bytesReceived:(NSUInteger)bytesReceived responseData:(NSData * _Null_unspecified)responseData traceHeaders:(NSDictionary * _Nullable)traceHeaders andParams:(NSDictionary * _Nullable)params;
        [Static]
        [Export("noticeNetworkRequestForURL:httpMethod:startTime:endTime:responseHeaders:statusCode:bytesSent:bytesReceived:responseData:traceHeaders:andParams:")]
        void NoticeNetworkRequestForURL(NSUrl url, string httpMethod, double startTime, double endTime, NSDictionary headers, nint httpStatusCode, nuint bytesSent, nuint bytesReceived, NSData responseData, [NullAllowed] NSDictionary traceHeaders, [NullAllowed] NSDictionary @params);

        // +(void)noticeNetworkFailureForURL:(NSURL * _Null_unspecified)url httpMethod:(NSString * _Null_unspecified)httpMethod withTimer:(NRTimer * _Null_unspecified)timer andFailureCode:(NSInteger)iOSFailureCode;
        [Static]
        [Export("noticeNetworkFailureForURL:httpMethod:withTimer:andFailureCode:")]
        void NoticeNetworkFailureForURL(NSUrl url, string httpMethod, NRTimer timer, nint iOSFailureCode);

        // +(void)noticeNetworkFailureForURL:(NSURL * _Null_unspecified)url httpMethod:(NSString * _Null_unspecified)httpMethod startTime:(double)startTime endTime:(double)endTime andFailureCode:(NSInteger)iOSFailureCode;
        [Static]
        [Export("noticeNetworkFailureForURL:httpMethod:startTime:endTime:andFailureCode:")]
        void NoticeNetworkFailureForURL(NSUrl url, string httpMethod, double startTime, double endTime, nint iOSFailureCode);

        // +(NSDictionary<NSString *,NSString *> * _Nonnull)generateDistributedTracingHeaders;
        [Static]
        [Export("generateDistributedTracingHeaders")]
        NSDictionary<NSString, NSString> GenerateDistributedTracingHeaders { get; }

        // +(BOOL)recordCustomEvent:(NSString * _Nonnull)eventType attributes:(NSDictionary * _Nullable)attributes;
        [Static]
        [Export("recordCustomEvent:attributes:")]
        bool RecordCustomEvent(string eventType, [NullAllowed] NSDictionary attributes);

        // +(BOOL)recordCustomEvent:(NSString * _Nonnull)eventType name:(NSString * _Nullable)name attributes:(NSDictionary * _Nullable)attributes;
        [Static]
        [Export("recordCustomEvent:name:attributes:")]
        bool RecordCustomEvent(string eventType, [NullAllowed] string name, [NullAllowed] NSDictionary attributes);

        // +(BOOL)recordBreadcrumb:(NSString * _Nonnull)name attributes:(NSDictionary * _Nullable)attributes;
        [Static]
        [Export("recordBreadcrumb:attributes:")]
        bool RecordBreadcrumb(string name, [NullAllowed] NSDictionary attributes);

        // +(void)setMaxEventBufferTime:(unsigned int)seconds;
        [Static]
        [Export("setMaxEventBufferTime:")]
        void SetMaxEventBufferTime(uint seconds);

        // +(void)setMaxEventPoolSize:(unsigned int)size;
        [Static]
        [Export("setMaxEventPoolSize:")]
        void SetMaxEventPoolSize(uint size);
        
        // +(void)setMaxOfflineStorageSize:(unsigned int)megaBytes;
        [Static]
        [Export ("setMaxOfflineStorageSize:")]
        void SetMaxOfflineStorageSize (uint megaBytes);

        // +(BOOL)setAttribute:(NSString * _Nonnull)name value:(id _Nonnull)value;
        [Static]
        [Export("setAttribute:value:")]
        bool SetAttribute(string name, NSObject value);

        // +(BOOL)incrementAttribute:(NSString * _Nonnull)name;
        [Static]
        [Export("incrementAttribute:")]
        bool IncrementAttribute(string name);

        // +(BOOL)incrementAttribute:(NSString * _Nonnull)name value:(NSNumber * _Nonnull)amount;
        [Static]
        [Export("incrementAttribute:value:")]
        bool IncrementAttribute(string name, NSNumber amount);

        // +(BOOL)setUserId:(NSString * _Nonnull)userId;
        [Static]
        [Export("setUserId:")]
        bool SetUserId(string userId);

        // +(BOOL)removeAttribute:(NSString * _Nonnull)name;
        [Static]
        [Export("removeAttribute:")]
        bool RemoveAttribute(string name);

        // +(BOOL)removeAllAttributes;
        [Static]
        [Export("removeAllAttributes")]
        bool RemoveAllAttributes { get; }

        // +(void)recordHandledException:(NSException * _Nonnull)exception;
        [Static]
        [Export("recordHandledException:")]
        void RecordHandledException(NSException exception);

        // +(void)recordHandledException:(NSException * _Nonnull)exception withAttributes:(NSDictionary * _Nullable)attributes;
        [Static]
        [Export("recordHandledException:withAttributes:")]
        void RecordHandledException(NSException exception, [NullAllowed] NSDictionary attributes);

        // +(void)recordHandledExceptionWithStackTrace:(NSDictionary * _Nonnull)exceptionDictionary;
        [Static]
        [Export("recordHandledExceptionWithStackTrace:")]
        void RecordHandledExceptionWithStackTrace(NSDictionary exceptionDictionary);

        // +(void)recordError:(NSError * _Nonnull)error;
        [Static]
        [Export("recordError:")]
        void RecordError(NSError error);

        // +(void)recordError:(NSError * _Nonnull)error attributes:(NSDictionary * _Nullable)attributes;
        [Static]
        [Export("recordError:attributes:")]
        void RecordError(NSError error, [NullAllowed] NSDictionary attributes);

        // +(void)shutdown();
        [Static]
        [Export("shutdown")]
        void Shutdown();

        // +(void)setLogIngestKey:(NSString *)key;
        [Static]
        [Export("setLogIngestKey:")]
        void SetLogIngestKey(string key);

        // +(void)setLogURL:(NSString *)url;
        [Static]
        [Export("setLogURL:")]
        void SetLogURL(string url);

        // +(void)upload;
        [Static]
        [Export("upload")]
        void Upload();

        // +(void)logInfo:(NSString * _Nonnull)message;
        [Static]
        [Export("logInfo:")]
        void LogInfo(string message);

        // +(void)logError:(NSString * _Nonnull)message;
        [Static]
        [Export("logError:")]
        void LogError(string message);

        // +(void)logVerbose:(NSString * _Nonnull)message;
        [Static]
        [Export("logVerbose:")]
        void LogVerbose(string message);

        // +(void)logWarning:(NSString * _Nonnull)message;
        [Static]
        [Export("logWarning:")]
        void LogWarning(string message);

        // +(void)logAudit:(NSString * _Nonnull)message;
        [Static]
        [Export("logAudit:")]
        void LogAudit(string message);

        // +(void)addHTTPHeaderTrackingFor:(NSArray<NSString *> * _Nonnull)headers;
        [Static]
        [Export("addHTTPHeaderTrackingFor:")]
        void AddHTTPHeaderTrackingFor(string[] headers);
    }

    // @interface NewRelicAgent : NewRelic
    [BaseType(typeof(NewRelic))]
    [Protocol]
    interface NewRelicAgent
    {
    }

    // @interface NRCustomMetrics : NSObject
    [BaseType(typeof(NSObject))]
    [Protocol]
    interface NRCustomMetrics
    {
        // +(void)recordMetricWithName:(NSString *)name category:(NSString *)category;
        [Static]
        [Export("recordMetricWithName:category:")]
        void RecordMetricWithName(string name, string category);

        // +(void)recordMetricWithName:(NSString *)name category:(NSString *)category value:(NSNumber *)value;
        [Static]
        [Export("recordMetricWithName:category:value:")]
        void RecordMetricWithName(string name, string category, NSNumber value);

        // +(void)recordMetricWithName:(NSString *)name category:(NSString *)category value:(NSNumber *)value valueUnits:(NRMetricUnit *)valueUnits;
        [Static]
        [Export("recordMetricWithName:category:value:valueUnits:")]
        void RecordMetricWithName(string name, string category, NSNumber value, string valueUnits);

        // +(void)recordMetricWithName:(NSString *)name category:(NSString *)category value:(NSNumber *)value valueUnits:(NRMetricUnit *)valueUnits countUnits:(NRMetricUnit *)countUnits;
        [Static]
        [Export("recordMetricWithName:category:value:valueUnits:countUnits:")]
        void RecordMetricWithName(string name, string category, NSNumber value, string valueUnits, string countUnits);
    }


    // @interface NRURLSessionTaskDelegateBase : NSObject <NSURLSessionTaskDelegate, NSURLSessionDataDelegate>
    [BaseType(typeof(NSObject))]
    [Protocol]
    interface NRURLSessionTaskDelegateBase : INSUrlSessionTaskDelegate, INSUrlSessionDataDelegate
    {
        [Wrap("WeakRealDelegate")]
        NSUrlSessionDataDelegate RealDelegate { get; }
        // @property (readonly, retain, nonatomic) id<NSURLSessionDataDelegate> realDelegate;
        [NullAllowed, Export("realDelegate", ArgumentSemantic.Retain)]
        NSObject WeakRealDelegate { get; }
    }
    // @interface NRWKNavigationDelegateBase : NSObject
    [BaseType(typeof(NSObject))]
    [Protocol]
    interface NRWKNavigationDelegateBase
    {
        [Wrap("WeakRealDelegate")]
        [NullAllowed]
        NSObject RealDelegate { get; set; }
        // @property (weak) NSObject * _Nullable realDelegate;
        [NullAllowed, Export("realDelegate", ArgumentSemantic.Weak)]
        NSObject WeakRealDelegate { get; set; }
    }
}
