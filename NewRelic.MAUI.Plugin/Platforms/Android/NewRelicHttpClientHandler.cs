/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Com.Newrelic.Agent.Android.Distributedtracing;
using NRAndroidAgent = Com.Newrelic.Agent.Android.NewRelic;

namespace NewRelic.MAUI.Plugin
{
    public class NewRelicHttpClientHandler : HttpClientHandler
    {
        private string TRACE_PARENT = "traceparent";
        private string TRACE_STATE = "tracestate";
        public NewRelicHttpClientHandler()
        {

        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            HttpResponseMessage httpResponseMessage;
            TraceContext traceContext = NRAndroidAgent.NoticeDistributedTrace(null);
            request.Headers.Add(traceContext.TracePayload.HeaderName, traceContext.TracePayload.HeaderValue);
            request.Headers.Add(TRACE_PARENT, "00-" + traceContext.TraceId + "-" + traceContext.ParentId + "-00");
            request.Headers.Add(TRACE_STATE, traceContext.Vendor + "=0-2-" + traceContext.AccountId + "-" + traceContext.ApplicationId + "-" + traceContext.ParentId + "----" + DateTimeOffset.Now.ToUnixTimeMilliseconds());
            httpResponseMessage = await base.SendAsync(request, cancellationToken);
            var endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            NRAndroidAgent.NoticeHttpTransaction(request.RequestUri.ToString(), request.Method.ToString(), ((int)httpResponseMessage.StatusCode), startTime, endTime, 0, httpResponseMessage.ToString().Length, "", null, null, traceContext.AsTraceAttributes());
            return httpResponseMessage;
        }

    }
}
