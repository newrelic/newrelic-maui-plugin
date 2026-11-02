/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0
 */

using Microsoft.Maui.Controls;

namespace NewRelic.MAUI.Plugin
{
    /// <summary>
    /// Fluent helpers that mark a <see cref="VisualElement"/> with the privacy tags the New Relic
    /// native agents look for when recording Session Replay.
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

        private static readonly BindableProperty PrivacyTagProperty = BindableProperty.CreateAttached(
            "NewRelicPrivacyTag", typeof(string), typeof(NewRelicPrivacyExtensions), null);

        // Writes the tag to the native view. A field so tests can observe it without a real platform view.
        internal static Action<VisualElement, string> PlatformApplier = ApplyToPlatformView;

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
            => (string)element?.GetValue(PrivacyTagProperty);

        private static T Apply<T>(T element, string tag) where T : VisualElement
        {
            if (element == null) return null;

            element.SetValue(PrivacyTagProperty, tag);

            // Unsubscribing first keeps repeated calls from stacking subscriptions.
            element.HandlerChanged -= OnHandlerChanged;
            element.HandlerChanged += OnHandlerChanged;

            if (element.Handler != null) PlatformApplier(element, tag);
            return element;
        }

        private static void OnHandlerChanged(object sender, EventArgs e)
        {
            var element = (VisualElement)sender;
            var tag = element.GetNewRelicPrivacyTag();
            if (tag != null && element.Handler != null) PlatformApplier(element, tag);
        }

        private static void ApplyToPlatformView(VisualElement element, string tag)
        {
#if IOS || ANDROID
            PrivacyPlatform.Apply(element, tag);
#endif
        }
    }
}
