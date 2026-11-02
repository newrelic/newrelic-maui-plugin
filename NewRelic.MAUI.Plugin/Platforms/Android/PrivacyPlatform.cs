/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0
 */

using AView = Android.Views.View;

namespace NewRelic.MAUI.Plugin;

// All the code in this file is only included on Android.
internal static class PrivacyPlatform
{
    // The agent reads the tag stored under its own R.id.newrelic_privacy, which is merged into the app's
    // resources, so the id is resolved by name rather than compiled in.
    private const string PrivacyTagResourceName = "newrelic_privacy";

    public static void Apply(VisualElement element, string tag)
    {
        if (element.Handler?.PlatformView is not AView view || view.Context == null) return;

        var id = view.Context.Resources?.GetIdentifier(PrivacyTagResourceName, "id", view.Context.PackageName) ?? 0;
        if (id == 0) return;

        view.SetTag(id, new Java.Lang.String(tag));
    }
}
