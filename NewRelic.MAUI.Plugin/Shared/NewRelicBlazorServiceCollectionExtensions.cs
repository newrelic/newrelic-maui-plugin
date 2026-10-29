/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

#nullable enable

namespace NewRelic.MAUI.Plugin
{
    public static class NewRelicBlazorServiceCollectionExtensions
    {
        public static IServiceCollection AddNewRelicBlazorExceptionHandler(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, NewRelicBlazorLoggerProvider>());
            return services;
        }

        private sealed class NewRelicBlazorLoggerProvider : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName)
            {
                return new NewRelicBlazorLogger(
                    categoryName == "Microsoft.AspNetCore.Components.Web.ErrorBoundary");
            }

            public void Dispose()
            {
            }
        }

        private sealed class NewRelicBlazorLogger : ILogger
        {
            private readonly bool _isBlazorErrorBoundary;

            public NewRelicBlazorLogger(bool isBlazorErrorBoundary)
            {
                _isBlazorErrorBoundary = isBlazorErrorBoundary;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
            {
                return _isBlazorErrorBoundary && logLevel >= Microsoft.Extensions.Logging.LogLevel.Error;
            }

            public void Log<TState>(
                Microsoft.Extensions.Logging.LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (exception != null && IsEnabled(logLevel))
                {
                    CrossNewRelic.Current.RecordException(exception);
                }
            }
        }
    }
}
