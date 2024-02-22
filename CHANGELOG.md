# Changelog

# 0.0.5

New in this release
- Added agent configuration for events, tracing, and FedRAMP compliance.
- Disabled default interaction tracing due to a crash.
- Resolved an issue causing app crashes when users utilized our plugin with a collection view.


# 0.0.4

- Fixed Some issues related iOS Dsyms Script

# 0.0.3
New in this release
- Adds configurable request header instrumentation to network events The agent will now produce network event attributes for select header values if - the headers are detected on the request. The header names to instrument are passed into the agent when started.
- Updated the native Android agent to version 7.2.0.
- Updated the native iOS agent to version 7.4.8.

# 0.0.2
New in this release
- Added Support for .net 6.0

# 0.0.1
The mobile agent team is proud to announce GA support for our .NET MAUI agent!
# New in this release
- Capture C# errors and crashes to quickly identify and fix problems
- Track network requests to see how your app is interacting with the backend
- Use distributed tracing to drill down into handled exceptions and identify the root cause of problems
- Create custom tracking events and metrics to fully understand how your users are interacting with your app