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

    // The id is fixed for the life of the process, so look it up once. Zero (not found) is never cached.
    private static int _privacyTagId;

    // Sets the tag, or removes it when tag is null.
    public static void Apply(VisualElement element, string tag)
    {
        if (element.Handler?.PlatformView is not AView view || view.Context == null) return;

        var id = ResolvePrivacyTagId(view);
        if (id == 0) return;

        if (tag == null)
        {
            view.SetTag(id, null);
            return;
        }

        // The view keeps its own reference to the Java string; the managed wrapper can be released now.
        using var javaTag = new Java.Lang.String(tag);
        view.SetTag(id, javaTag);
    }

    private static int ResolvePrivacyTagId(AView view)
    {
        if (_privacyTagId != 0) return _privacyTagId;

        var context = view.Context;
        var id = context?.Resources?.GetIdentifier(PrivacyTagResourceName, "id", context.PackageName) ?? 0;
        _privacyTagId = id;
        return id;
    }
}
