/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0
 */

using System;
using System.Runtime.InteropServices;
using CoreFoundation;
using ObjCRuntime;

namespace MauiiOS.NewRelic
{
	[Flags]
	public enum NRMAFeatureFlags : ulong
	{
		InteractionTracing = 1uL << 1,
		SwiftInteractionTracing = 1uL << 2,
		CrashReporting = 1uL << 3,
		NSURLSessionInstrumentation = 1uL << 4,
		HttpResponseBodyCapture = 1uL << 5,
		WebViewInstrumentation = 1uL << 7,
		RequestErrorEvents = 1uL << 8,
		NetworkRequestEvents = 1uL << 9,
		HandledExceptionEvents = 1uL << 10,
		DefaultInteractions = 1uL << 12,
		ExperimentalNetworkingInstrumentation = 1uL << 13,
		DistributedTracing = 1uL << 14,
		GestureInstrumentation = 1uL << 15,
		AppStartMetrics = 1uL << 16,
		FedRampEnabled = 1uL << 17,
		SwiftAsyncURLSessionSupport = 1uL << 18,
		NewEventSystem = 1uL << 20,
		OfflineStorage = 1uL << 21,
		BackgroundReporting = 1uL << 22
	}

	[Native]
	public enum NRMAApplicationPlatform : ulong
	{
		Native,
		Cordova,
		PhoneGap,
		Xamarin,
		Unity,
		Appcelerator,
		ReactNative,
		Flutter,
		Capacitor,
		Maui,
		Unreal
	}

	public enum NRTraceType : uint
	{
		None,
		ViewLoading,
		Layout,
		Database,
		Images,
		Json,
		Network
	}

	public enum NRNetworkFailureCode
	{
		Unknown = -1,
		Cancelled = -999,
		BadURL = -1000,
		TimedOut = -1001,
		UnsupportedURL = -1002,
		CannotFindHost = -1003,
		CannotConnectToHost = -1004,
		DataLengthExceedsMaximum = -1103,
		NetworkConnectionLost = -1005,
		DNSLookupFailed = -1006,
		HTTPTooManyRedirects = -1007,
		ResourceUnavailable = -1008,
		NotConnectedToInternet = -1009,
		RedirectToNonExistentLocation = -1010,
		BadServerResponse = -1011,
		UserCancelledAuthentication = -1012,
		UserAuthenticationRequired = -1013,
		ZeroByteResource = -1014,
		CannotDecodeRawData = -1015,
		CannotDecodeContentData = -1016,
		CannotParseResponse = -1017,
		InternationalRoamingOff = -1018,
		CallIsActive = -1019,
		DataNotAllowed = -1020,
		RequestBodyStreamExhausted = -1021,
		FileDoesNotExist = -1100,
		FileIsDirectory = -1101,
		NoPermissionsToReadFile = -1102,
		SecureConnectionFailed = -1200,
		ServerCertificateHasBadDate = -1201,
		ServerCertificateUntrusted = -1202,
		ServerCertificateHasUnknownRoot = -1203,
		ServerCertificateNotYetValid = -1204,
		ClientCertificateRejected = -1205,
		ClientCertificateRequired = -1206,
		CannotLoadFromNetwork = -2000,
		CannotCreateFile = -3000,
		CannotOpenFile = -3001,
		CannotCloseFile = -3002,
		CannotWriteToFile = -3003,
		CannotRemoveFile = -3004,
		CannotMoveFile = -3005,
		DownloadDecodingFailedMidStream = -3006,
		DownloadDecodingFailedToComplete = -3007
	}
	

	public enum NRLogLevels : uint
	{
		None = 0,
		Error = 1 << 0,
		Warning = 1 << 1,
		Info = 1 << 2,
		Verbose = 1 << 3,
		Audit = 1 << 4,
		Debug = 1 << 5,
		All = 65535
	}

	public enum NRLogTargets : uint
	{
		None = 0,
		Console = 1 << 0,
		File = 1 << 1
	}
}
