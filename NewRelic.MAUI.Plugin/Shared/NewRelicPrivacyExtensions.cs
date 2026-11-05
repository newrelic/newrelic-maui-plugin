/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0
 */

using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace NewRelic.MAUI.Plugin
{
    /// <summary>
    /// Marks a <see cref="VisualElement"/> with the privacy tags the New Relic native agents look for
    /// when recording Session Replay. Use the fluent extension methods in code:
    /// <code>var password = new Entry { IsPassword = true }.NewRelicMask();</code>
    /// or the <see cref="PrivacyTagProperty"/> attached property in XAML:
    /// <code>&lt;Entry nr:NewRelicPrivacyExtensions.PrivacyTag="nr-mask" /&gt;</code>
    /// where <c>nr</c> maps to <c>clr-namespace:NewRelic.MAUI.Plugin;assembly=NewRelic.MAUI.Plugin</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tag is written to the element's native view once its handler exists, and again whenever the
    /// handler is replaced. Android agents read the <c>newrelic_privacy</c> view tag; the iOS agent reads
    /// <c>accessibilityIdentifier</c>.
    /// </para>
    /// <para>
    /// <see cref="Element.AutomationId"/> is never modified (MAUI only allows it to be set once). On iOS
    /// this means a tagged element's native <c>accessibilityIdentifier</c> is the privacy tag rather than
    /// its <c>AutomationId</c>, so UI tests should locate tagged elements another way.
    /// </para>
    /// An element carries at most one privacy tag; applying another replaces it.
    /// </remarks>
    public static class NewRelicPrivacyExtensions
    {
        public const string MaskTag = "nr-mask";
        public const string UnmaskTag = "nr-unmask";
        public const string BlockTag = "nr-block";

        /// <summary>
        /// Attached property holding the privacy tag (<see cref="MaskTag"/>, <see cref="UnmaskTag"/>,
        /// <see cref="BlockTag"/>, or null for none). The fluent extension methods set this property.
        /// </summary>
        public static readonly BindableProperty PrivacyTagProperty = BindableProperty.CreateAttached(
            "PrivacyTag", typeof(string), typeof(NewRelicPrivacyExtensions), null,
            propertyChanged: OnPrivacyTagChanged);

        // Writes the tag (or clears it, for null) on the native view. A field so tests can observe it
        // without a real platform view.
        internal static Action<VisualElement, string> PlatformApplier = ApplyToPlatformView;

        private static int _automationIdHookRegistered;

        /// <summary>Gets the privacy tag on <paramref name="bindable"/>, or null if it has none.</summary>
        public static string GetPrivacyTag(BindableObject bindable)
            => (string)bindable?.GetValue(PrivacyTagProperty);

        /// <summary>Sets the privacy tag on <paramref name="bindable"/>; null removes it.</summary>
        public static void SetPrivacyTag(BindableObject bindable, string value)
            => bindable?.SetValue(PrivacyTagProperty, value);

        /// <summary>Masks the element's content in Session Replay.</summary>
        public static T NewRelicMask<T>(this T element) where T : VisualElement
            => Apply(element, MaskTag);

        /// <summary>Forces the element's content to be shown unmasked in Session Replay.</summary>
        public static T NewRelicUnmask<T>(this T element) where T : VisualElement
            => Apply(element, UnmaskTag);

        /// <summary>Blocks the element from Session Replay entirely.</summary>
        public static T NewRelicBlock<T>(this T element) where T : VisualElement
            => Apply(element, BlockTag);

        /// <summary>Returns the privacy tag applied to the element, or null if it has none.</summary>
        public static string GetNewRelicPrivacyTag(this VisualElement element)
            => GetPrivacyTag(element);

        private static T Apply<T>(T element, string tag) where T : VisualElement
        {
            if (element == null) return null;

            SetPrivacyTag(element, tag);
            return element;
        }

        private static void OnPrivacyTagChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not VisualElement element) return;

#if IOS
            EnsureAutomationIdHook();
#endif

            // Unsubscribing first keeps repeated changes from stacking subscriptions.
            element.HandlerChanged -= OnHandlerChanged;
            element.HandlerChanged += OnHandlerChanged;

            if (element.Handler != null) PlatformApplier(element, (string)newValue);
        }

        private static void OnHandlerChanged(object sender, EventArgs e)
        {
            var element = (VisualElement)sender;
            var tag = GetPrivacyTag(element);

            // A new handler starts without a tag, so there is nothing to clear for an untagged element.
            if (tag != null && element.Handler != null) PlatformApplier(element, tag);
        }

        // MAUI's AutomationId mapper writes the native identifier right after PropertyChanged
        // (iOS: accessibilityIdentifier), which would silently replace the tag if AutomationId is set
        // after the element was tagged. Re-apply the tag once that mapper has run.
        internal static void EnsureAutomationIdHook()
        {
            if (Interlocked.Exchange(ref _automationIdHookRegistered, 1) == 1) return;

            ViewHandler.ViewMapper.ModifyMapping(nameof(Microsoft.Maui.IView.AutomationId), (handler, view, previous) =>
            {
                previous?.Invoke(handler, view);

                if (view is VisualElement element && element.Handler != null)
                {
                    var tag = GetPrivacyTag(element);
                    if (tag != null) PlatformApplier(element, tag);
                }
            });
        }

        private static void ApplyToPlatformView(VisualElement element, string tag)
        {
#if IOS || ANDROID
            PrivacyPlatform.Apply(element, tag);
#endif
        }
    }
}
