using NewRelic.MAUI.Plugin;
using NUnit.Framework;
using Moq;

namespace NewRelic.MAUI.Plugin.Tests;

[TestFixture]
public class INewRelicMethodsTest
{
    Mock<INewRelicMethods> newRelicMethodsMock;
    NewRelicMethodsMock newRelicMockScope;

    [SetUp]
    public void Setup()
    {
        newRelicMethodsMock = new Mock<INewRelicMethods>();
        newRelicMockScope = new NewRelicMethodsMock(newRelicMethodsMock.Object);
    }

    [Test]
    public void TestStart()
    {
        CrossNewRelic.Current.Start("<app-token>");
        AgentStartConfiguration asc = new AgentStartConfiguration();
        CrossNewRelic.Current.Start("<app-token-2>", asc);
        Assert.That(asc.offlineStorageEnabled.Equals(true));
        Assert.That(asc.fedRampEnabled.Equals(false));
        Assert.That(asc.interactionTracingEnabled.Equals(false));
        newRelicMethodsMock.Verify(methods => methods.Start("<app-token>", null), Times.Once);
        newRelicMethodsMock.Verify(methods => methods.Start("<app-token-2>", asc), Times.Once);
    }


    [Test]
    public void TestCrashNow()
    {
        CrossNewRelic.Current.CrashNow();
        CrossNewRelic.Current.CrashNow("Example crash");
        newRelicMethodsMock.Verify(methods => methods.CrashNow(""), Times.Once);
        newRelicMethodsMock.Verify(methods => methods.CrashNow("Example crash"), Times.Once);
    }

    [Test]
    public void TestCurrentSessionId()
    {
        string id = CrossNewRelic.Current.CurrentSessionId();
        newRelicMethodsMock.Verify(methods => methods.CurrentSessionId(), Times.Once);
    }

    [Test]
    public void TestStartInteraction()
    {
        string id = CrossNewRelic.Current.StartInteraction("interactionName");
        newRelicMethodsMock.Verify(methods => methods.StartInteraction("interactionName"), Times.Once);
    }

    [Test]
    public void TestEndInteraction()
    {
        CrossNewRelic.Current.EndInteraction("interactionId");
        newRelicMethodsMock.Verify(methods => methods.EndInteraction("interactionId"), Times.Once);
    }

    [Test]
    public void TestNoticeHttpTransaction()
    {
        CrossNewRelic.Current.NoticeHttpTransaction(
            "https://newrelic.com",
            "GET",
            200,
            10000,
            10001,
            0,
            100,
            "");
        newRelicMethodsMock.Verify(methods => methods.NoticeHttpTransaction(
            "https://newrelic.com",
            "GET",
            200,
            10000,
            10001,
            0,
            100,
            ""), Times.Once);
    }

    [Test]
    public void TestNoticeNetworkFailure()
    {
        CrossNewRelic.Current.NoticeNetworkFailure(
            "https://fakewebsite.com",
            "POST",
            10000,
            10001,
            NetworkFailure.Unknown);
        newRelicMethodsMock.Verify(methods => methods.NoticeNetworkFailure(
            "https://fakewebsite.com",
            "POST",
            10000,
            10001,
            NetworkFailure.Unknown), Times.Once);
    }

    [Test]
    public void TestRecordBreadcrumb()
    {
        Dictionary<string, object> attr = new Dictionary<string, object>()
        {
            { "attr1", 1 },
            { "attr2", "2" },
            { "attr3", false }
        };
        CrossNewRelic.Current.RecordBreadcrumb("Breadname", attr);
        newRelicMethodsMock.Verify(methods => methods.RecordBreadcrumb("Breadname", attr), Times.Once);
    }

    [Test]
    public void TestRecordCustomEvent()
    {
        Dictionary<string, object> attr = new Dictionary<string, object>()
        {
            { "attr1", 1 },
            { "attr2", "2" },
            { "attr3", false }
        };
        CrossNewRelic.Current.RecordCustomEvent("EventType", "EventName", attr);
        newRelicMethodsMock.Verify(methods => methods.RecordCustomEvent("EventType", "EventName", attr), Times.Once);
    }

    [Test]
    public void TestRecordMetric()
    {
        CrossNewRelic.Current.RecordMetric("Metric0", "TestMetric");
        CrossNewRelic.Current.RecordMetric("Metric1", "TestMetric", 12.3);
        CrossNewRelic.Current.RecordMetric("Metric2", "TestMetric", 45.6, MetricUnit.PERCENT, MetricUnit.BYTES_PER_SECOND);

        newRelicMethodsMock.Verify(methods => methods.RecordMetric("Metric0", "TestMetric"), Times.Once);
        newRelicMethodsMock.Verify(methods => methods.RecordMetric("Metric1", "TestMetric", 12.3), Times.Once);
        newRelicMethodsMock.Verify(methods => methods.RecordMetric("Metric2", "TestMetric", 45.6, MetricUnit.PERCENT, MetricUnit.BYTES_PER_SECOND), Times.Once);
    }

    [Test]
    public void TestSetAttribute()
    {
        CrossNewRelic.Current.SetAttribute("StrAttr", "MAUIAttr");
        CrossNewRelic.Current.SetAttribute("BoolAttr", true);
        CrossNewRelic.Current.SetAttribute("NumAttr", 78.9);

        newRelicMethodsMock.Verify(methods => methods.SetAttribute("StrAttr", "MAUIAttr"), Times.Once);
        newRelicMethodsMock.Verify(methods => methods.SetAttribute("BoolAttr", true), Times.Once);
        newRelicMethodsMock.Verify(methods => methods.SetAttribute("NumAttr", 78.9), Times.Once);
    }

    [Test]
    public void TestIncrementAttribute()
    {
        CrossNewRelic.Current.IncrementAttribute("NumAttr");
        CrossNewRelic.Current.IncrementAttribute("NumAttr", 3);

        newRelicMethodsMock.Verify(methods => methods.IncrementAttribute("NumAttr", 1), Times.Once);
        newRelicMethodsMock.Verify(methods => methods.IncrementAttribute("NumAttr", 3), Times.Once);
    }

    [Test]
    public void TestRemoveAttribute()
    {
        CrossNewRelic.Current.RemoveAttribute("NumAttr");
        newRelicMethodsMock.Verify(methods => methods.RemoveAttribute("NumAttr"), Times.Once);
    }

    [Test]
    public void TestRemoveAllAttributes()
    {
        CrossNewRelic.Current.RemoveAllAttributes();
        newRelicMethodsMock.Verify(methods => methods.RemoveAllAttributes(), Times.Once);
    }

    [Test]
    public void TestSetMaxEventBufferTime()
    {
        CrossNewRelic.Current.SetMaxEventBufferTime(120);
        newRelicMethodsMock.Verify(methods => methods.SetMaxEventBufferTime(120), Times.Once);
    }
    
    [Test]
    public void TestSetMaxOfflineStorageSize()
    {
        CrossNewRelic.Current.SetMaxOfflineStorageSize(120);
        newRelicMethodsMock.Verify(methods => methods.SetMaxOfflineStorageSize(120), Times.Once);
    }

    [Test]
    public void TestSetMaxEventPoolSize()
    {
        CrossNewRelic.Current.SetMaxEventPoolSize(1200);
        newRelicMethodsMock.Verify(methods => methods.SetMaxEventPoolSize(1200), Times.Once);
    }

    [Test]
    public void TestSetUserId()
    {
        CrossNewRelic.Current.SetUserId("abc");
        newRelicMethodsMock.Verify(methods => methods.SetUserId("abc"), Times.Once);
    }

    [Test]
    public void TestAnalyticsEventEnabled()
    {
        CrossNewRelic.Current.AnalyticsEventEnabled(true);
        newRelicMethodsMock.Verify(methods => methods.AnalyticsEventEnabled(true), Times.Once);
    }

    [Test]
    public void TestNetworkRequestEnabled()
    {
        CrossNewRelic.Current.NetworkRequestEnabled(true);
        newRelicMethodsMock.Verify(methods => methods.NetworkRequestEnabled(true), Times.Once);
    }

    [Test]
    public void TestNetworkErrorRequestEnabled()
    {
        CrossNewRelic.Current.NetworkErrorRequestEnabled(true);
        newRelicMethodsMock.Verify(methods => methods.NetworkErrorRequestEnabled(true), Times.Once);
    }

    [Test]
    public void TestHttpResponseBodyCaptureEnabled()
    {
        CrossNewRelic.Current.HttpResponseBodyCaptureEnabled(true);
        newRelicMethodsMock.Verify(methods => methods.HttpResponseBodyCaptureEnabled(true), Times.Once);
    }

    [Test]
    public void TestRecordException()
    {
        Exception testException = new Exception("test exception");
        CrossNewRelic.Current.RecordException(testException);
        newRelicMethodsMock.Verify(methods => methods.RecordException(testException), Times.Once);
    }

    [Test]
    public void TestGetHttpMessageHandler()
    {
        HttpMessageHandler httpMessageHandler = CrossNewRelic.Current.GetHttpMessageHandler();
        newRelicMethodsMock.Verify(methods => methods.GetHttpMessageHandler(), Times.Once);
    }

    [Test]
    public void TestHandleUncaughtException()
    {
        CrossNewRelic.Current.HandleUncaughtException();
        CrossNewRelic.Current.HandleUncaughtException(false);
        newRelicMethodsMock.Verify(methods => methods.HandleUncaughtException(true), Times.Once);
        newRelicMethodsMock.Verify(methods => methods.HandleUncaughtException(false), Times.Once);
    }

    [Test]
    public void TestTrackShellNavigatedEvents()
    {
        CrossNewRelic.Current.TrackShellNavigatedEvents();
        newRelicMethodsMock.Verify(methods => methods.TrackShellNavigatedEvents(), Times.Once);
    }

    [Test]
    public void TestShutdown()
    {
        CrossNewRelic.Current.Shutdown();
        newRelicMethodsMock.Verify(methods => methods.Shutdown(), Times.Once);
    }
}
