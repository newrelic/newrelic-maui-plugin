[![Community Plus header](https://github.com/newrelic/opensource-website/raw/main/src/images/categories/Community_Plus.png)](https://opensource.newrelic.com/oss-category/#community-plus)

[![nuget](https://img.shields.io/nuget/v/NewRelic.MAUI.Plugin)](https://www.nuget.org/packages/NewRelic.MAUI.Plugin)

# New Relic MAUI Plugin

This plugin allows you to instrument .NET MAUI mobile apps with help of native New Relic Android and iOS Bindings. The New Relic SDKs collect crashes, network traffic, and other information for hybrid apps using native components.


## Features

* Capture Android and iOS Crashes
* Network Request tracking
* Distributed Tracing
* Pass user information to New Relic to track user sessions
* Screen Tracking

## Current Support:

This project targets .NET MAUI mobile apps and supports .NET MAUI [minimum supported platforms](https://learn.microsoft.com/en-us/dotnet/maui/supported-platforms):
- Android 7.0 (API 24) or higher
- iOS 11 or higher, using the latest release of Xcode
- Depends on New Relic iOS/XCFramework and Android agents

## Installation

Install NewRelic plugin into your MAUI project by adding the NuGet Package `NewRelic.MAUI.Plugin`.

Open your solution, select the project you want to add NewRelic package to and open its context menu. Unfold "Add" and click "Add NuGet packages...".

## MAUI Setup

1. Open your `App.xaml.cs` and add the following code to launch NewRelic Plugin (don't forget to put proper application tokens):

```C#
using NewRelic.MAUI.Plugin;
...
    public App ()
    {
      InitializeComponent();

      MainPage = new AppShell();
      
      CrossNewRelic.Current.HandleUncaughtException();
      CrossNewRelic.Current.TrackShellNavigatedEvents();

      // Set optional agent configuration
      // Options are: crashReportingEnabled, loggingEnabled, logLevel, collectorAddress, crashCollectorAddress
      // AgentStartConfiguration agentConfig = new AgentStartConfiguration(true, true, LogLevel.INFO, "mobile-collector.newrelic.com", "mobile-crash.newrelic.com");

      if (DeviceInfo.Current.Platform == DevicePlatform.Android) 
      {
        CrossNewRelic.Current.Start("<APP-TOKEN-HERE>");
        // Start with optional agent configuration 
        // CrossNewRelic.Current.Start("<APP-TOKEN-HERE", agentConfig);
      } else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
      {
        CrossNewRelic.Current.Start("<APP-TOKEN-HERE>");
        // Start with optional agent configuration 
        // CrossNewRelic.Current.Start("<APP-TOKEN-HERE", agentConfig);
      }
    }

```

## Android Setup

1. Open `Platforms/Android/AndroidManifest.xml` for your Android App and add the following permissions:

```xml
	<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
	<uses-permission android:name="android.permission.INTERNET" />
```


## Screen Tracking Events

The .NET MAUI mobile plugin allows you to track navigation events within the [.NET MAUI Shell](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation). In order to do so, you only need to call:

```C#
    CrossNewRelic.Current.TrackShellNavigatedEvents();
```

It is recommended to call this method along when starting the agent. These events will only be recorded after navigation is complete. You can find this data through the data explorer in `MobileBreadcrumb` under the name `ShellNavigated` or by query:

```sql
    SELECT * FROM MobileBreadcrumb WHERE name = 'ShellNavigated' SINCE 24 HOURS AGO
```

The breadcrumb will contain three attributes:
* `Current`: The URI of the current page.
* `Source`: The type of navigation that occurred.
* `Previous`: The URI of the previous page. Will not exist if previous page was null. 

## Usage

See the examples below, and for more detail,
see [New Relic iOS SDK doc](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-ios/ios-sdk-api)
or [Android SDK](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api)
.

### [CrashNow](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/crashnow-android-sdk-api/)(string message = "") : void;

> Throws a demo run-time exception on Android/iOS to test New Relic crash reporting.

``` C#
    CrossNewRelic.Current.CrashNow();
```

### [CurrentSessionId](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/currentsessionid-android-sdk-api/)() : string;

> Returns ID for the current session.

``` C#
    string sessionId = CrossNewRelic.Current.CurrentSessionId();
```

### [StartInteraction](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/start-interaction)(string interactionName): string;

> Track a method as an interaction.


### [EndInteraction](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/end-interaction)(string interactionId): void;

> End an interaction
> (Required). This uses the string ID for the interaction you want to end.
> This string is returned when you use startInteraction().

``` C#
    HttpClient myClient = new HttpClient(CrossNewRelic.Current.GetHttpMessageHandler());
    
    string interactionId = CrossNewRelic.Current.StartInteraction("Getting data from service");

    var response = await myClient.GetAsync(new Uri("https://jsonplaceholder.typicode.com/todos/1"));
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
    } else
    {
        Console.WriteLine("Unsuccessful response code");
    }

    CrossNewRelic.Current.EndInteraction(interactionId);

```

### [NoticeHttpTransaction](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/notice-http-transaction/)(string url, string httpMethod, int statusCode, long startTime,long endTime, long bytesSent, long bytesReceived, string responseBody): void;

> Tracks network requests manually. You can use this method to record HTTP transactions, with an option to also send a response body.

``` C#
    CrossNewRelic.Current.NoticeHttpTransaction(
      "https://newrelic.com",
      "GET",
      200,
      DateTimeOffset.Now.ToUnixTimeMilliseconds(),
      DateTimeOffset.Now.ToUnixTimeMilliseconds() + 100,
      0,
      1000,
      ""
    );
```

### [NoticeNetworkFailure](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/notice-network-failure/)(string url, string httpMethod, int statusCode, long startTime,long endTime, long bytesSent, long bytesReceived, string responseBody): void;

> Records network failures. If a network request fails, use this method to record details about the failure.

``` C#
    CrossNewRelic.Current.NoticeNetworkFailure(
      "https://fakewebsite.com",
      "GET",
      DateTimeOffset.Now.ToUnixTimeMilliseconds(),
      DateTimeOffset.Now.ToUnixTimeMilliseconds() + 100,
      NetworkFailure.Unknown
    );
```

### [RecordBreadcrumb](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/recordbreadcrumb)(string name, Dictionary<string, object> attributes): bool;

> This call creates and records a MobileBreadcrumb event, which can be queried with NRQL and in the crash event trail.

``` C#
    CrossNewRelic.Current.RecordBreadcrumb("MAUIExampleBreadcrumb", new Dictionary<string, object>()
        {
            {"BreadNumValue", 12.3 },
            {"BreadStrValue", "MAUIBread" },
            {"BreadBoolValue", true }
        }
    );
```

### [RecordCustomEvent](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/recordcustomevent-android-sdk-api)(string eventType, string eventName, Dictionary<string, object> attributes): bool;

> Creates and records a custom event for use in New Relic Insights.

``` C#
    CrossNewRelic.Current.RecordCustomEvent("MAUICustomEvent", "MAUICustomEventCategory", new Dictionary<string, object>()
        {
            {"BreadNumValue", 12.3 },
            {"BreadStrValue", "MAUIBread" },
            {"BreadBoolValue", true }
        }
    );
```

### [RecordMetric](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/recordmetric-android-sdk-api/)(string name, string category) : void;
### [RecordMetric](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/recordmetric-android-sdk-api/)(string name, string category, double value) : void;

> Record custom metrics (arbitrary numerical data).

``` C#
    CrossNewRelic.Current.RecordMetric("Agent start", "Lifecycle");
    CrossNewRelic.Current.RecordMetric("Login Auth Metric", "Network", 78.9);
```

### [SetAttribute](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/set-attribute)(string name, string value) : bool;
### [SetAttribute](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/set-attribute)(string name, double value) : bool;
### [SetAttribute](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/set-attribute)(string name, bool value) : bool;

> Creates a session-level attribute shared by multiple mobile event types. Overwrites its previous value and type each time it is called.

``` C#
    CrossNewRelic.Current.SetAttribute("MAUIBoolAttr", false);
    CrossNewRelic.Current.SetAttribute("MAUIStrAttr", "Cat");
    CrossNewRelic.Current.SetAttribute("MAUINumAttr", 13.5);
```

### [IncrementAttribute](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/set-attribute)(string name, float value = 1) : bool;

> Increments the count of an attriubte. Overwrites its previous value and type each time it is called.

``` C#
    // Increment by 1
    CrossNewRelic.Current.IncrementAttribute("MAUINumAttr");
    // Increment by value
    CrossNewRelic.Current.IncrementAttribute("MAUINumAttr", 12.3);
```

### [RemoveAttribute](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/remove-attribute)(string name) : bool;

> Removes an attribute.

``` C#
    CrossNewRelic.Current.RemoveAttribute("MAUINumAttr");
```

### [RemoveAllAttributes](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/remove-all-attributes)() : bool;

> Removes all attributes from the session.

``` C#
    CrossNewRelic.Current.RemoveAllAttributes();
```

### [SetMaxEventBufferTime](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/set-max-event-buffer-time)(int maxBufferTimeInSec) void;

> Sets the event harvest cycle length.

  ``` C#
      CrossNewRelic.Current.SetMaxEventBufferTime(200);
  ```
### [SetMaxEventPoolSize](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/set-max-event-pool-size)(int maxPoolSize): void;

> Sets the maximum size of the event pool.

  ``` C#
      CrossNewRelic.Current.SetMaxEventPoolSize(1500);
  ```

### [SetUserId](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/set-user-id)(string userId): bool;

> Set a custom user identifier value to associate user sessions with analytics events and attributes.

``` C#
    CrossNewRelic.Current.SetUserId("User123");
```

### GetHttpMessageHandler() : HttpMessageHandler;

> Provides a HttpMessageHandler to instrument http requests through HttpClient.

``` C#
    HttpClient myClient = new HttpClient(CrossNewRelic.Current.GetHttpMessageHandler());

    var response = await myClient.GetAsync(new Uri("https://jsonplaceholder.typicode.com/todos/1"));
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
    } else
    {
        Console.WriteLine("Http request failed");
    }
```

### AnalyticsEventEnabled(bool enabled) : void

> FOR ANDROID ONLY. Enable or disable collection of event data.

``` C#
    CrossNewRelic.Current.AnalyticsEventEnabled(true);
```

### NetworkRequestEnabled(bool enabled) : void

> Enable or disable reporting successful HTTP requests to the MobileRequest event type.

``` C#
    CrossNewRelic.Current.NetworkRequestEnabled(true);
```

### NetworkErrorRequestEnabled(bool enabled) : void

> Enable or disable reporting network and HTTP request errors to the MobileRequestError event type.

``` C#
    CrossNewRelic.Current.NetworkErrorRequestEnabled(true);
```

### HttpResponseBodyCaptureEnabled(bool enabled) : void

> Enable or disable capture of HTTP response bodies for HTTP error traces, and MobileRequestError events.

``` C#
    CrossNewRelic.Current.HttpResponseBodyCaptureEnabled(true);
```

### Shutdown() : void

> Shut down the agent within the current application lifecycle during runtime.
``` C#
    CrossNewRelic.Current.Shutdown());
```

## Error reporting

This plugin provides a handler to record unhandled exceptions to New Relic. It is recommended to initialize the handler prior to starting the agent.

### HandleUncaughtException(bool shouldThrowFormattedException = true) : void;

``` C#
    CrossNewRelic.Current.HandleUncaughtException();
    if (DeviceInfo.Current.Platform == DevicePlatform.Android) 
    {
        CrossNewRelic.Current.Start("<APP-TOKEN-HERE>");
    } else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
    {
        CrossNewRelic.Current.Start("<APP-TOKEN-HERE>");
    }
```

This plugin also provides a method to manually record any handled exceptions as well:
### RecordException(System.Exception exception) : void;

``` C#
    try {
      some_code_that_throws_error();
    } catch (Exception ex) {
      CrossNewRelic.Current.RecordException(ex);
    }
```


## Troubleshooting

- ### No Http data appears:
  - To instrument http data, make sure to use the HttpMessageHandler in HttpClient.

## Support

New Relic hosts and moderates an online forum where customers, users, maintainers, contributors, and New Relic employees can discuss and collaborate:

[forum.newrelic.com](https://forum.newrelic.com/).

## Contribute

We encourage your contributions to improve [project name]! Keep in mind that when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project.

If you have any questions, or to execute our corporate CLA (which is required if your contribution is on behalf of a company), drop us an email at opensource@newrelic.com.

**A note about vulnerabilities**

As noted in our [security policy](../../security/policy), New Relic is committed to the privacy and security of our customers and their data. We believe that providing coordinated disclosure by security researchers and engaging with the security community are important means to achieve our security goals.

If you believe you have found a security vulnerability in this project or any of New Relic's products or websites, we welcome and greatly appreciate you reporting it to New Relic through [HackerOne](https://hackerone.com/newrelic).

If you would like to contribute to this project, review [these guidelines](./CONTRIBUTING.md).

To all contributors, we thank you!  Without your contribution, this project would not be what it is today.

## License
Except as described below, the `newrelic-maui-plugin` is licensed under the [Apache 2.0](http://apache.org/licenses/LICENSE-2.0.txt) License.

The [New Relic XCFramework agent] (/docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-ios/get-started/introduction-new-relic-mobile-ios/) is licensed under the [New Relic Agent Software Notice] (/docs.newrelic.com/docs/licenses/license-information/distributed-licenses/new-relic-agent-software-notice/).

The [New Relic Android agent] (github.com/newrelic/newrelic-android-agent) is licensed under the [Apache 2.0](http://apache.org/licenses/LICENSE-2.0.txt).

The `newrelic-maui-plugin` may use source code from third-party libraries. When used, these libraries will be outlined in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
