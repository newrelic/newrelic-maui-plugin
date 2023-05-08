/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

using System;
using Java.Lang;
using plugin.NRTest;

namespace Plugin.NRTest
{
	internal class NewRelicMauiException : Java.Lang.Exception
    {
        public NewRelicMauiException(string message, StackTraceElement[] stackTrace) : base(message)
        {
            SetStackTrace(stackTrace);
        }

        public static NewRelicMauiException Create(System.Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            var message = $"{exception.GetType()}: {exception.Message}";

            var stackTrace = StackTraceParser.Parse(exception)
                .Select(frame => new StackTraceElement(frame.ClassName, frame.MethodName, frame.FileName, frame.LineNumber))
                .ToArray();


            return new NewRelicMauiException(message, stackTrace);
        }
    }
}


