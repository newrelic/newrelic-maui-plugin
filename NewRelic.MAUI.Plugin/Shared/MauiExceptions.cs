/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

using System;
namespace NewRelic.MAUI.Plugin
{
    public static class MauiExceptions
    {
#if WINDOWS
    private static Exception _lastFirstChanceException;
#endif

        // Guards against reporting the same unhandled exception twice when more than one hook
        // fires for it (on Mono both AppDomain.CurrentDomain.UnhandledException and
        // ObjCRuntime.Runtime.MarshalManagedException fire for the same iOS exception). (NR-588069)
        private static Exception _lastReportedException;

        // We'll route all unhandled exceptions through this one event.
        public static event UnhandledExceptionEventHandler UnhandledException;

        static MauiExceptions()
        {
            // This is the normal event expected, and should still be used.
            // It will fire for exceptions from iOS and Mac Catalyst,
            // and for exceptions on background threads from WinUI 3.

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    if (ReferenceEquals(ex, _lastReportedException))
                    {
                        return;
                    }
                    _lastReportedException = ex;
                }
                UnhandledException?.Invoke(sender, args);
            };

#if IOS || MACCATALYST

        // For iOS and Mac Catalyst:
        //
        // On Mono, unhandled exceptions flowed through AppDomain.CurrentDomain.UnhandledException
        // (we set UnwindNativeCode below to make that work).
        // See: https://github.com/xamarin/xamarin-macios/issues/15252
        //
        // Under CoreCLR (.NET 10+), AppDomain.CurrentDomain.UnhandledException no longer fires for
        // exceptions that unwind through native code, so we also capture the exception here at the
        // managed->native boundary, where CoreCLR surfaces it (args.Exception is populated). Verified
        // on-device: for an unhandled managed exception only MarshalManagedException fires, not AppDomain.
        // See: https://learn.microsoft.com/en-us/dotnet/ios/advanced-concepts/exception-marshaling (NR-588069)
        ObjCRuntime.Runtime.MarshalManagedException += (_, args) =>
        {
            if (args.Exception is not null && !ReferenceEquals(args.Exception, _lastReportedException))
            {
                _lastReportedException = args.Exception;
                UnhandledException?.Invoke(null, new UnhandledExceptionEventArgs(args.Exception, true));
            }

            args.ExceptionMode = ObjCRuntime.MarshalManagedExceptionMode.UnwindNativeCode;
        };

#elif ANDROID

        // For Android:
        // All exceptions will flow through Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser,
        // and NOT through AppDomain.CurrentDomain.UnhandledException

        global::Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            UnhandledException?.Invoke(sender, new UnhandledExceptionEventArgs(args.Exception, true));
        };

#elif WINDOWS

        // For WinUI 3:
        //
        // * Exceptions on background threads are caught by AppDomain.CurrentDomain.UnhandledException,
        //   not by Microsoft.UI.Xaml.Application.Current.UnhandledException
        //   See: https://github.com/microsoft/microsoft-ui-xaml/issues/5221
        //
        // * Exceptions caught by Microsoft.UI.Xaml.Application.Current.UnhandledException have details removed,
        //   but that can be worked around by saved by trapping first chance exceptions
        //   See: https://github.com/microsoft/microsoft-ui-xaml/issues/7160
        //

        AppDomain.CurrentDomain.FirstChanceException += (_, args) =>
        {
            _lastFirstChanceException = args.Exception;
        };

        Microsoft.UI.Xaml.Application.Current.UnhandledException += (sender, args) =>
        {
            var exception = args.Exception;

            if (exception.StackTrace is null)
            {
                exception = _lastFirstChanceException;
            }

            UnhandledException?.Invoke(sender, new UnhandledExceptionEventArgs(exception, true));
        };
#endif
        }
    }
}

