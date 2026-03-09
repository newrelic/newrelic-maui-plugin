#!/bin/bash

# Update iOS XCFramework Info.plist
# This script removes watchOS and tvOS entries from the Info.plist
# Usage: ./update-ios-xcframework.sh <path-to-Info.plist>

set -e

PLIST_PATH="$1"

if [ -z "$PLIST_PATH" ]; then
    echo "Usage: $0 <path-to-Info.plist>"
    exit 1
fi

if [ ! -f "$PLIST_PATH" ]; then
    echo "Error: File not found: $PLIST_PATH"
    exit 1
fi

# Use Python to process the plist (works on both macOS and Linux)
python3 << EOF
import plistlib
import sys

plist_path = "$PLIST_PATH"

try:
    with open(plist_path, 'rb') as f:
        plist = plistlib.load(f)
except Exception as e:
    print(f"Error reading plist: {e}")
    sys.exit(1)

# Filter AvailableLibraries to keep only iOS entries (exclude maccatalyst variant)
if 'AvailableLibraries' in plist:
    original_count = len(plist['AvailableLibraries'])
    plist['AvailableLibraries'] = [
        lib for lib in plist['AvailableLibraries']
        if lib.get('SupportedPlatform', '').lower() == 'ios'
        and lib.get('SupportedPlatformVariant', '').lower() != 'maccatalyst'
    ]
    filtered_count = len(plist['AvailableLibraries'])
    print(f"Filtered libraries: {original_count} -> {filtered_count} (removed {original_count - filtered_count} non-iOS/maccatalyst entries)")

try:
    with open(plist_path, 'wb') as f:
        plistlib.dump(plist, f)
    print(f"Successfully updated {plist_path}")
except Exception as e:
    print(f"Error writing plist: {e}")
    sys.exit(1)
EOF
