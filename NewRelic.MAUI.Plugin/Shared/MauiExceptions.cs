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
        // fires for it. On Android, AppDomain.CurrentDomain.UnhandledException and
        // AndroidEnvironment.UnhandledExceptionRaiser both fire for the same exception (confirmed
        // on-device on both MonoVM and CoreCLR) — without this guard, RecordException runs twice.
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

        // For iOS and Mac Catalyst
        // Exceptions will flow through AppDomain.CurrentDomain.UnhandledException,
        // but we need to set UnwindNativeCode to get it to work correctly. 
        // 
        // See: https://github.com/xamarin/xamarin-macios/issues/15252
        
        ObjCRuntime.Runtime.MarshalManagedException += (_, args) =>
        {
            args.ExceptionMode = ObjCRuntime.MarshalManagedExceptionMode.UnwindNativeCode;
        };

#elif ANDROID

        // For Android:
        // Exceptions flow through Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser.
        // AppDomain.CurrentDomain.UnhandledException also fires for the same exception (confirmed
        // on-device on both MonoVM and CoreCLR), so guard against double-reporting via the shared
        // _lastReportedException check above.

        global::Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            if (args.Exception is not null && !ReferenceEquals(args.Exception, _lastReportedException))
            {
                _lastReportedException = args.Exception;
                UnhandledException?.Invoke(sender, new UnhandledExceptionEventArgs(args.Exception, true));
            }
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

