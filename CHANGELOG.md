# Changelog

# 0.0.7

- Updated native iOS Agent: We've upgraded the native iOS agent to version 7.4.10, which includes performance improvements and bug fixes.


# 0.0.6

New in this release
- Added Offline Harvesting Feature: This new feature enables the preservation of harvest data that would otherwise be lost when the application lacks an internet connection. The stored harvests will be sent once the internet connection is re-established and the next harvest upload is successful.
- Introduced setMaxOfflineStorageSize API: This new API allows the user to determine the maximum volume of data that can be stored locally. This aids in better management and control of local data storage.
- Updated native iOS Agent: We've upgraded the native iOS agent to version 7.4.9, which includes performance improvements and bug fixes.
- Updated native Android Agent: We've also upgraded the native Android agent to version 7.3.0 bringing benefits like improved stability and enhanced features.

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