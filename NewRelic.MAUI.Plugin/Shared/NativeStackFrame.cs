/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

using System;
namespace Plugin.NRTest
{
    internal class NativeStackFrame
    {
        public NativeStackFrame(string className, string methodName, string fileName, int lineNumber)
        {
            ClassName = className;
            MethodName = methodName;
            FileName = fileName;
            LineNumber = lineNumber;
        }

        public string ClassName { get; }
        public string MethodName { get; }
        public string FileName { get; }
        public int LineNumber { get; }
    }
}

