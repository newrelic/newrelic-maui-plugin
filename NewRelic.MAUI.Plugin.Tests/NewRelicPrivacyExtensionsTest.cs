/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0
 */

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Moq;
using NewRelic.MAUI.Plugin;
using NUnit.Framework;

namespace NewRelic.MAUI.Plugin.Tests;

[TestFixture]
public class NewRelicPrivacyExtensionsTest
{
    private Action<VisualElement, string> originalApplier = null!;
    private List<(VisualElement Element, string Tag)> applied = null!;

    [SetUp]
    public void Setup()
    {
        originalApplier = NewRelicPrivacyExtensions.PlatformApplier;
        applied = new List<(VisualElement, string)>();
        NewRelicPrivacyExtensions.PlatformApplier = (element, tag) => applied.Add((element, tag));
    }

    [TearDown]
    public void TearDown()
    {
        NewRelicPrivacyExtensions.PlatformApplier = originalApplier;
    }

    private static IViewHandler NewHandler() => new Mock<IViewHandler>().Object;

    [Test]
    public void Constants_MatchNativeAgentTags()
    {
        Assert.That(NewRelicPrivacyExtensions.MaskTag, Is.EqualTo("nr-mask"));
        Assert.That(NewRelicPrivacyExtensions.UnmaskTag, Is.EqualTo("nr-unmask"));
        Assert.That(NewRelicPrivacyExtensions.BlockTag, Is.EqualTo("nr-block"));
    }

    // Tagging

    [Test]
    public void Element_WithoutTagging_HasNoPrivacyTag()
    {
        Assert.That(new Entry().GetNewRelicPrivacyTag(), Is.Null);
    }

    [Test]
    public void Mask_SetsMaskTag()
    {
        Assert.That(new Entry().NewRelicMask().GetNewRelicPrivacyTag(), Is.EqualTo("nr-mask"));
    }

    [Test]
    public void Unmask_SetsUnmaskTag()
    {
        Assert.That(new Entry().NewRelicUnmask().GetNewRelicPrivacyTag(), Is.EqualTo("nr-unmask"));
    }

    [Test]
    public void Block_SetsBlockTag()
    {
        Assert.That(new Entry().NewRelicBlock().GetNewRelicPrivacyTag(), Is.EqualTo("nr-block"));
    }

    // Existing automation ids are never touched (MAUI only allows AutomationId to be set once)

    [Test]
    public void Tagging_DoesNotTouchUnsetAutomationId()
    {
        var entry = new Entry().NewRelicMask();

        Assert.That(entry.AutomationId, Is.Null);
    }

    [Test]
    public void Tagging_PreservesExistingAutomationId()
    {
        var entry = new Entry { AutomationId = "login_email_field" };

        entry.NewRelicMask().NewRelicUnmask().NewRelicBlock();

        Assert.That(entry.AutomationId, Is.EqualTo("login_email_field"));
    }

    // Switching states

    [Test]
    public void Unmask_AfterMask_ReplacesTag()
    {
        var entry = new Entry().NewRelicMask().NewRelicUnmask();

        Assert.That(entry.GetNewRelicPrivacyTag(), Is.EqualTo("nr-unmask"));
    }

    [Test]
    public void Mask_AfterUnmask_ReplacesTag()
    {
        var entry = new Entry().NewRelicUnmask().NewRelicMask();

        Assert.That(entry.GetNewRelicPrivacyTag(), Is.EqualTo("nr-mask"));
    }

    [Test]
    public void Block_AfterMask_ReplacesTag()
    {
        var entry = new Entry().NewRelicMask().NewRelicBlock();

        Assert.That(entry.GetNewRelicPrivacyTag(), Is.EqualTo("nr-block"));
    }

    [Test]
    public void Mask_AfterBlock_ReplacesTag()
    {
        var entry = new Entry().NewRelicBlock().NewRelicMask();

        Assert.That(entry.GetNewRelicPrivacyTag(), Is.EqualTo("nr-mask"));
    }

    // Idempotency

    [Test]
    public void Mask_CalledTwice_KeepsSingleMaskTag()
    {
        var entry = new Entry().NewRelicMask().NewRelicMask();

        Assert.That(entry.GetNewRelicPrivacyTag(), Is.EqualTo("nr-mask"));
    }

    [Test]
    public void Mask_CalledTwiceWithHandler_AppliesToPlatformViewIdempotently()
    {
        var entry = new Entry { Handler = NewHandler() };

        entry.NewRelicMask().NewRelicMask();

        Assert.That(applied.Select(a => a.Tag), Is.All.EqualTo("nr-mask"));
    }

    // Safety and fluency

    [Test]
    public void NullElement_ReturnsNull()
    {
        Entry? entry = null;

        Assert.That(entry.NewRelicMask(), Is.Null);
        Assert.That(entry.NewRelicUnmask(), Is.Null);
        Assert.That(entry.NewRelicBlock(), Is.Null);
    }

    [Test]
    public void NullElement_ReadingTag_ReturnsNull()
    {
        VisualElement? element = null;

        Assert.That(element.GetNewRelicPrivacyTag(), Is.Null);
    }

    [Test]
    public void Methods_ReturnSameInstanceOfSameType()
    {
        var entry = new Entry { IsPassword = true };

        Entry masked = entry.NewRelicMask();

        Assert.That(masked, Is.SameAs(entry));
        Assert.That(masked.IsPassword, Is.True);
    }

    [Test]
    public void Methods_WorkOnAnyVisualElement()
    {
        var label = new Label().NewRelicMask();
        var layout = new VerticalStackLayout().NewRelicBlock();

        Assert.That(label.GetNewRelicPrivacyTag(), Is.EqualTo("nr-mask"));
        Assert.That(layout.GetNewRelicPrivacyTag(), Is.EqualTo("nr-block"));
    }

    [Test]
    public void Tags_AreIndependentPerElement()
    {
        var masked = new Entry().NewRelicMask();
        var unmasked = new Entry().NewRelicUnmask();

        Assert.That(masked.GetNewRelicPrivacyTag(), Is.EqualTo("nr-mask"));
        Assert.That(unmasked.GetNewRelicPrivacyTag(), Is.EqualTo("nr-unmask"));
    }

    // Native view timing: the tag is written to the platform view once a handler exists

    [Test]
    public void Tagging_WithoutHandler_DoesNotTouchPlatformView()
    {
        new Entry().NewRelicMask();

        Assert.That(applied, Is.Empty);
    }

    [Test]
    public void Tagging_WithExistingHandler_AppliesImmediately()
    {
        var entry = new Entry { Handler = NewHandler() };

        entry.NewRelicMask();

        Assert.That(applied, Has.Count.EqualTo(1));
        Assert.That(applied[0].Element, Is.SameAs(entry));
        Assert.That(applied[0].Tag, Is.EqualTo("nr-mask"));
    }

    [Test]
    public void Tagging_BeforeHandlerAttaches_AppliesWhenHandlerAttaches()
    {
        var entry = new Entry().NewRelicMask();

        entry.Handler = NewHandler();

        Assert.That(applied, Has.Count.EqualTo(1));
        Assert.That(applied[0].Tag, Is.EqualTo("nr-mask"));
    }

    [Test]
    public void Tagging_ReappliesWhenHandlerIsReplaced()
    {
        var entry = new Entry().NewRelicMask();
        entry.Handler = NewHandler();

        entry.Handler = NewHandler();

        Assert.That(applied, Has.Count.EqualTo(2));
    }

    [Test]
    public void Tagging_DoesNotApplyWhenHandlerIsRemoved()
    {
        var entry = new Entry().NewRelicMask();
        entry.Handler = NewHandler();
        applied.Clear();

        entry.Handler = null;

        Assert.That(applied, Is.Empty);
    }

    [Test]
    public void Tagging_RepeatedCallsDoNotStackHandlerSubscriptions()
    {
        var entry = new Entry().NewRelicMask().NewRelicMask().NewRelicUnmask();

        entry.Handler = NewHandler();

        Assert.That(applied, Has.Count.EqualTo(1));
        Assert.That(applied[0].Tag, Is.EqualTo("nr-unmask"));
    }

    [Test]
    public void Switching_AfterHandlerAttached_AppliesNewTag()
    {
        var entry = new Entry { Handler = NewHandler() }.NewRelicMask();

        entry.NewRelicUnmask();

        Assert.That(applied.Select(a => a.Tag), Is.EqualTo(new[] { "nr-mask", "nr-unmask" }));
    }

    [Test]
    public void ElementNeverTagged_NeverTouchesPlatformView()
    {
        var entry = new Entry();

        entry.Handler = NewHandler();

        Assert.That(applied, Is.Empty);
    }

    // XAML-friendly attached property (fluent methods are thin wrappers over it)

    [Test]
    public void PrivacyTagProperty_IsPublicAttachedPropertyNamedPrivacyTag()
    {
        Assert.That(NewRelicPrivacyExtensions.PrivacyTagProperty.PropertyName, Is.EqualTo("PrivacyTag"));
        Assert.That(NewRelicPrivacyExtensions.PrivacyTagProperty.ReturnType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void SetPrivacyTag_SetsTagReadableByBothGetters()
    {
        var entry = new Entry();

        NewRelicPrivacyExtensions.SetPrivacyTag(entry, "nr-mask");

        Assert.That(NewRelicPrivacyExtensions.GetPrivacyTag(entry), Is.EqualTo("nr-mask"));
        Assert.That(entry.GetNewRelicPrivacyTag(), Is.EqualTo("nr-mask"));
    }

    [Test]
    public void GetPrivacyTag_NullElement_ReturnsNull()
    {
        Assert.That(NewRelicPrivacyExtensions.GetPrivacyTag(null), Is.Null);
    }

    [Test]
    public void FluentMethods_AreWrappersOverTheAttachedProperty()
    {
        var entry = new Entry().NewRelicBlock();

        Assert.That(entry.GetValue(NewRelicPrivacyExtensions.PrivacyTagProperty), Is.EqualTo("nr-block"));
    }

    [Test]
    public void SetPrivacyTag_BeforeHandlerAttaches_AppliesWhenHandlerAttaches()
    {
        var entry = new Entry();
        NewRelicPrivacyExtensions.SetPrivacyTag(entry, "nr-unmask");

        entry.Handler = NewHandler();

        Assert.That(applied.Select(a => a.Tag), Is.EqualTo(new[] { "nr-unmask" }));
    }

    [Test]
    public void SetPrivacyTag_WithExistingHandler_AppliesImmediately()
    {
        var entry = new Entry { Handler = NewHandler() };

        NewRelicPrivacyExtensions.SetPrivacyTag(entry, "nr-mask");

        Assert.That(applied.Select(a => a.Tag), Is.EqualTo(new[] { "nr-mask" }));
    }

    [Test]
    public void SetPrivacyTag_SameValueTwice_AppliesOnce()
    {
        var entry = new Entry { Handler = NewHandler() };

        NewRelicPrivacyExtensions.SetPrivacyTag(entry, "nr-mask");
        NewRelicPrivacyExtensions.SetPrivacyTag(entry, "nr-mask");

        Assert.That(applied, Has.Count.EqualTo(1));
    }

    [Test]
    public void SetPrivacyTag_NullWithHandler_ClearsTheNativeTag()
    {
        var entry = new Entry { Handler = NewHandler() }.NewRelicMask();

        NewRelicPrivacyExtensions.SetPrivacyTag(entry, null);

        Assert.That(entry.GetNewRelicPrivacyTag(), Is.Null);
        Assert.That(applied.Select(a => a.Tag), Is.EqualTo(new string[] { "nr-mask", null }));
    }

    [Test]
    public void SetPrivacyTag_NullWithoutHandler_DoesNotTouchPlatformView()
    {
        var entry = new Entry().NewRelicMask();

        NewRelicPrivacyExtensions.SetPrivacyTag(entry, null);

        Assert.That(applied, Is.Empty);
    }

    [Test]
    public void ClearedTag_IsNotReappliedWhenHandlerChanges()
    {
        var entry = new Entry().NewRelicMask();
        NewRelicPrivacyExtensions.SetPrivacyTag(entry, null);

        entry.Handler = NewHandler();

        Assert.That(applied, Is.Empty);
    }

    // AutomationId set after tagging: MAUI's AutomationId mapper rewrites the native identifier
    // (it runs right after PropertyChanged), so the tag has to be re-applied once it has run.

    [Test]
    public void AutomationIdMapped_OnTaggedElement_ReappliesTag()
    {
        NewRelicPrivacyExtensions.EnsureAutomationIdHook();
        var handler = NewHandler();
        var entry = new Entry { Handler = handler }.NewRelicMask();
        applied.Clear();

        ViewHandler.ViewMapper.UpdateProperty(handler, entry, nameof(IView.AutomationId));

        Assert.That(applied.Select(a => a.Tag), Is.EqualTo(new[] { "nr-mask" }));
    }

    [Test]
    public void AutomationIdMapped_OnUntaggedElement_DoesNothing()
    {
        NewRelicPrivacyExtensions.EnsureAutomationIdHook();
        var handler = NewHandler();
        var entry = new Entry { Handler = handler };

        ViewHandler.ViewMapper.UpdateProperty(handler, entry, nameof(IView.AutomationId));

        Assert.That(applied, Is.Empty);
    }

    [Test]
    public void AutomationIdHook_RegisteredTwice_StillAppliesOncePerMapping()
    {
        NewRelicPrivacyExtensions.EnsureAutomationIdHook();
        NewRelicPrivacyExtensions.EnsureAutomationIdHook();
        var handler = NewHandler();
        var entry = new Entry { Handler = handler }.NewRelicMask();
        applied.Clear();

        ViewHandler.ViewMapper.UpdateProperty(handler, entry, nameof(IView.AutomationId));

        Assert.That(applied, Has.Count.EqualTo(1));
    }

    [Test]
    public void AutomationIdSetAfterTagging_TagWinsOverTheMappedIdentifier()
    {
        // Reproduces MAUI's order: PropertyChanged, then handler.UpdateValue -> property mapper.
        NewRelicPrivacyExtensions.EnsureAutomationIdHook();
        var mock = new Mock<IViewHandler>();
        Entry entry = null!;
        mock.Setup(h => h.UpdateValue(It.IsAny<string>()))
            .Callback<string>(property => ViewHandler.ViewMapper.UpdateProperty(mock.Object, entry, property));
        entry = new Entry { Handler = mock.Object }.NewRelicMask();
        applied.Clear();

        entry.AutomationId = "login_email";

        Assert.That(entry.AutomationId, Is.EqualTo("login_email"));
        Assert.That(applied.Select(a => a.Tag), Is.EqualTo(new[] { "nr-mask" }));
    }
}
