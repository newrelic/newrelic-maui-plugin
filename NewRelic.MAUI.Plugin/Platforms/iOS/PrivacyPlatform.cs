/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0
 */

using Microsoft.Maui.Controls;
using UIKit;

namespace NewRelic.MAUI.Plugin;

// All the code in this file is only included on iOS.
internal static class PrivacyPlatform
{
    // Sets the tag, or (for null) restores the identifier MAUI maps from AutomationId.
    public static void Apply(VisualElement element, string tag)
    {
        // Replaces the identifier MAUI mapped from AutomationId; the agent matches on this property.
        if (element.Handler?.PlatformView is UIView view) view.AccessibilityIdentifier = tag ?? element.AutomationId;
    }
}
