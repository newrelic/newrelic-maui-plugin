//
// NRLogger
// NewRelic
//
//  New Relic for Mobile -- iOS edition
//
//  See:
//    https://docs.newrelic.com/docs/mobile-monitoring for information
//    https://docs.newrelic.com/docs/release-notes/mobile-release-notes/xcframework-release-notes/ for release notes
//
//  Copyright © 2023 New Relic. All rights reserved.
//  See https://docs.newrelic.com/docs/licenses/ios-agent-licenses for license details
//

#import <Foundation/Foundation.h>

#ifdef __cplusplus
extern "C" {
#endif

#ifndef _NEWRELIC_AGENT_LOGGING_
#define _NEWRELIC_AGENT_LOGGING_

/*************************************/
/**      SDK Internal Logging       **/
/*************************************/

/*******************************************************************************
 * The New Relic agent includes an internal logger called NRLogger to make your
 * life a touch easier when you want to know what's going on under the hood.
 * You can direct various levels of agent activity messages to the device
 * console through NSLog or to a file stored in the app's document directory.
 *
 * Please note that NRLogger does not send any data whatsoever to New Relic's
 * servers. You'll need to have access to the device/simulator console or dig
 * the file out yourself.
 *******************************************************************************/


/*******************************************************************************
 * Log levels used in the agent's internal logger
 *
 * When calling NRLogger setLogLevels: pass in a bitmask of the levels you want
 * enabled, ORed together e.g.
 *   [NRLogger setLogLevels:NRLogLevelError|NRLogLevelWarning|NRLogLevelInfo];
 *
 * NRLogLevelALL is a convenience definition.
 *
 * NRLogger's default log level is NRLogLevelError|NRLogLevelWarning
 *******************************************************************************/

typedef enum _NRLogLevels {
    NRLogLevelNone    = 0,
    NRLogLevelError   = 1 << 0,
    NRLogLevelWarning = 1 << 1,
    NRLogLevelInfo    = 1 << 2,
    NRLogLevelVerbose = 1 << 3,
    NRLogLevelAudit   = 1 << 4,
    NRLogLevelDebug   = 1 << 5,
    NRLogLevelALL     = 0xffff
} NRLogLevels;

typedef enum _NRLogTargets {
    NRLogTargetNone      = 0,
    NRLogTargetConsole   = 1 << 0,
    NRLogTargetFile      = 1 << 1
} NRLogTargets;

#define NRLogMessageLevelKey        @"level"
#define NRLogMessageFileKey         @"file"
#define NRLogMessageLineNumberKey   @"lineNumber"
#define NRLogMessageMethodKey       @"method"
#define NRLogMessageTimestampKey    @"timestamp"
#define NRLogMessageMessageKey      @"message"
#define NRLogMessageSessionIdKey    @"sessionId"
#define NRLogMessageAppIdKey        @"appId"

#define NRLogMessageEntityGuidKey   @"entity.guid"
#define NRLogMessageInstrumentationProviderKey   @"instrumentation.provider"
#define NRLogMessageMobileValue     @"mobile"
#define NRLogMessageInstrumentationNameKey       @"instrumentation.name"
#define NRLogMessageInstrumentationVersionKey    @"instrumentation.version"
#define NRLogMessageInstrumentationCollectorKey  @"collector.name"

/*******************************************************************************
 * Log targets used in the agent's internal logger
 *
 * When calling NRLogger setLogTargets: pass in a bitmask of the targets you
 * want enabled, ORed together e.g.
 *   [NRLogger setLogTargets:NRLogTargetConsole|NRLogTargetFile];
 *
 * NRLogTargetConsole uses NSLog() to output to the device console
 * NRLogTargetFile writes log messages to a file in JSON-format
 * NRLogTargetALL is a convenience definition.
 *
 *NRLogger's default target is NRLogTargetConsole
 *******************************************************************************/

@interface NRLogger : NSObject {
    unsigned int logLevels;
    unsigned int logTargets;
    NSFileHandle *logFile;
    NSString *logURL;

    NSString *logIngestKey;
    NSString *logEntityGuid;

    dispatch_queue_t logQueue;
    unsigned long long lastFileSize;

    NSMutableArray *uploadQueue;
    BOOL isUploading;
    unsigned int failureCount;
    BOOL debugLogs;

    NRLogLevels remoteLogLevel;

}

+ (void)log:(unsigned int)level
     inFile:(NSString *)file
     atLine:(unsigned int)line
   inMethod:(NSString *)method
withMessage:(NSString *)message
withAttributes:(NSDictionary *)attributes;


+ (void)log:(unsigned int)level
     inFile:(NSString *)file
     atLine:(unsigned int)line
   inMethod:(NSString *)method
withMessage:(NSString *)message
withAgentLogsOn:(BOOL)agentLogsOn;

+ (void) log:(unsigned int)level
     withMessage:(NSString *)message
withTimestamp:(NSNumber *)timestamp;

/*!
 Configure the amount of information the New Relic agent outputs about its internal operation.
 
 @param levels A single NRLogLevels constant, or a bitwise ORed combination of NRLogLevels
 
 Note: If you provide a single constant, e.g. NRLogLevelInfo, all higher priority info will also be output.
 */
+ (void)setLogLevels:(unsigned int)levels;

/*!
 Configure the verbosity level of information the New Relic agent outputs over the network via the New Relic Logs API.

 @param level A single NRLogLevels constant,

 Note:  a single constant, e.g. NRLogLevelInfo, all higher priority info will also be output.

 */
+ (void)setRemoteLogLevel:(unsigned int)level;


/*!
 Configure the output channels to which the New Relic agent logs internal operation data.

 @param targets a bitwise ORed combination of NRLogTargets constants

 NRLogTargetConsole will output messages using NSLog()
 NRLogTargetFile will write log messages to a file on the device or simulator. Use logFilePath to retrieve the log file location.
 */
+ (void)setLogTargets:(unsigned int)targets;

// For internal use only. Do not use.
+ (void)setLogIngestKey:(NSString*)key;

// For internal use only. Do not use.
+ (void)setLogEntityGuid:(NSString*)key;

+ (void)setLogURL:(NSString*) url;

/*!
 @result the path of the file to which the New Relic agent is logging.

 The file contains comma-separated JSON blobs, each blob encapsulating one log message.
 */
+ (NSString *)logFilePath;

/*!
 @result the data of the file which the New Relic agent is logging.

 The data contains comma-separated JSON blobs, each blob encapsulating one log message.
 */
+ (NSData *)logFileData:(NSError **) errorPtr;

/*!
 Truncate the log file used by the New Relic agent for data logging.
 */
+ (void)clearLog;

/*!
 Enqueue current log since last upload.
 */
+ (void)enqueueLogUpload;


/*
 Convert NSString to NRLogLevel.
 */
+ (NRLogLevels)stringToLevel:(NSString*)string;

/*!
 return currently set logLevels
 */
+ (NRLogLevels) logLevels;

@end


#define NRLOG(level, agentLogs, format, ...) \
    [NRLogger log:level inFile:[[NSString stringWithUTF8String:__FILE__] lastPathComponent] atLine:__LINE__ inMethod:[NSString stringWithUTF8String:__func__] withMessage:[NSString stringWithFormat:format, ##__VA_ARGS__] withAgentLogsOn:agentLogs]

#define NRLOG_ATTRS(level, format, attrs, ...) \
    [NRLogger log:level inFile:[[NSString stringWithUTF8String:__FILE__] lastPathComponent] atLine:__LINE__ inMethod:[NSString stringWithUTF8String:__func__] withMessage:[NSString stringWithFormat:format, ##__VA_ARGS__] withAttributes: attrs]

#define NRLOG_ERROR(format, ...) NRLOG(NRLogLevelError, false, format, ##__VA_ARGS__)
#define NRLOG_WARNING(format, ...) NRLOG(NRLogLevelWarning, false, format, ##__VA_ARGS__)
#define NRLOG_INFO(format, ...) NRLOG(NRLogLevelInfo, false, format, ##__VA_ARGS__)
#define NRLOG_VERBOSE(format, ...) NRLOG(NRLogLevelVerbose, false, format, ##__VA_ARGS__)
#define NRLOG_AUDIT(format, ...) NRLOG(NRLogLevelAudit, false, format, ##__VA_ARGS__)
#define NRLOG_DEBUG(format, ...) NRLOG(NRLogLevelDebug, false, format, ##__VA_ARGS__)

#define NRLOG_AGENT_ERROR(format, ...) NRLOG(NRLogLevelError, true, format, ##__VA_ARGS__)
#define NRLOG_AGENT_WARNING(format, ...) NRLOG(NRLogLevelWarning, true, format, ##__VA_ARGS__)
#define NRLOG_AGENT_INFO(format, ...) NRLOG(NRLogLevelInfo, true, format, ##__VA_ARGS__)
#define NRLOG_AGENT_VERBOSE(format, ...) NRLOG(NRLogLevelVerbose, true, format, ##__VA_ARGS__)
#define NRLOG_AGENT_AUDIT(format, ...) NRLOG(NRLogLevelAudit, true, format, ##__VA_ARGS__)
#define NRLOG_AGENT_DEBUG(format, ...) NRLOG(NRLogLevelDebug, true, format, ##__VA_ARGS__)

#define NRLOG_ERROR_ATTRS(format, attrs, ...)   NRLOG_ATTRS(NRLogLevelError, format, attrs, ##__VA_ARGS__)
#define NRLOG_WARNING_ATTRS(format, attrs, ...) NRLOG_ATTRS(NRLogLevelWarning, format, attrs, ##__VA_ARGS__)
#define NRLOG_INFO_ATTRS(format, attrs, ...)    NRLOG_ATTRS(NRLogLevelInfo, format, attrs, ##__VA_ARGS__)
#define NRLOG_VERBOSE_ATTRS(format, attrs, ...) NRLOG_ATTRS(NRLogLevelVerbose, format, attrs, ##__VA_ARGS__)
#define NRLOG_AUDIT_ATTRS(format, attrs, ...)   NRLOG_ATTRS(NRLogLevelAudit, format, attrs, ##__VA_ARGS__)
#define NRLOG_DEBUG_ATTRS(format, attrs, ...)   NRLOG_ATTRS(NRLogLevelDebug, format, attrs, ##__VA_ARGS__)

#endif // _NEWRELIC_AGENT_LOGGING_

#ifdef __cplusplus
}
#endif
