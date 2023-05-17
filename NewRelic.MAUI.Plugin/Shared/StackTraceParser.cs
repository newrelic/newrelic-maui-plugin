/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NewRelic.MAUI.Plugin;

namespace NewRelic.MAUI.Plugin
{
    internal static class StackTraceParser
    {
#if DEBUG
        private static readonly Regex _regex =
           new Regex(@"\s*at (?<className>\S+)\.(?<methodName>\S+\(.*\))( in (?<fileName>.+)):line (?<lineNumber>\d+)*$",
                RegexOptions.Multiline | RegexOptions.ExplicitCapture);
#else
       private static readonly Regex _regex =
           new Regex(@"\s*at (?<className>\S+)\.(?<methodName>\S+\(.*\))( in (?<fileName>.+))*$",
                RegexOptions.Multiline | RegexOptions.ExplicitCapture);
#endif


        public static IEnumerable<NativeStackFrame> Parse(Exception exception)
        {
            var stackFrames = new List<NativeStackFrame>();

            ParseLocal(exception, stackFrames);

            return stackFrames;

            static void ParseLocal(Exception exception, List<NativeStackFrame> stackFrames)
            {
                if (exception == null) return;

                stackFrames.AddRange(Parse(exception.StackTrace));

                if (exception is AggregateException aggregateException)
                {
                    var number = 0;
                    foreach (var innerException in aggregateException.InnerExceptions)
                    {
                        var (namespaceName, className) = GetNamespaceNameAndClassName(innerException.GetType());

                        var methodName = string.Empty;

                        if (!string.IsNullOrEmpty(namespaceName))
                        {
                            methodName = $"{className}: {innerException.Message}";
                            className = $"(Inner Exception #{number++}) {namespaceName}";
                        }
                        else
                        {
                            className = $"(Inner Exception #{number++}) {className}: {innerException.Message}";
                        }

                        stackFrames.Add(new NativeStackFrame(className, methodName, "", 0));

                        ParseLocal(innerException, stackFrames);
                    }
                }
                else if (exception.InnerException != null)
                {
                    var (namespaceName, className) = GetNamespaceNameAndClassName(exception.InnerException.GetType());

                    var methodName = string.Empty;

                    if (!string.IsNullOrEmpty(namespaceName))
                    {
                        methodName = $"{className}: {exception.InnerException.Message}";
                        className = $"(Inner Exception) {namespaceName}";
                    }
                    else
                    {
                        className = $"(Inner Exception) {className}: {exception.InnerException.Message}";
                    }

                    stackFrames.Add(new NativeStackFrame(className, methodName, "", 0));

                    ParseLocal(exception.InnerException, stackFrames);
                }
            }
        }

        private static (string? NamespaceName, string ClassName) GetNamespaceNameAndClassName(Type type)
        {
            var className = type.ToString();
            var namespaceName = type.Namespace;

            if (!string.IsNullOrEmpty(namespaceName))
            {
                className = className.Substring(namespaceName.Length + 1);
            }

            return (namespaceName, className);
        }

        public static IEnumerable<NativeStackFrame> Parse(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace)) yield break;

            foreach (Match match in _regex.Matches(stackTrace))
            {
                var className = match.Groups["className"].Value;
                var methodName = match.Groups["methodName"].Value;

                var lineNumberGroup = match.Groups["lineNumber"];
                var lineNumber = lineNumberGroup.Success ? int.Parse(lineNumberGroup.Value) : 0;

                var fileNameGroup = match.Groups["fileName"];
                var fileName = fileNameGroup.Success && lineNumber > 0 ? fileNameGroup.Value : "<unknown>";

                yield return new NativeStackFrame(className, methodName, fileName, lineNumber);
            }
        }
    }
}