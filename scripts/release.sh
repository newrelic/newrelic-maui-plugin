#!/bin/bash

# NewRelic MAUI Plugin Release Script
# This script automates the release process for NewRelic MAUI Plugin
# Usage: ./scripts/release.sh --android-version X.X.X --ios-version X.X.X --plugin-version X.X.X [options]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Default values
DRY_RUN=false
SKIP_ANDROID=false
SKIP_IOS=false
SKIP_PLUGIN=false
PUBLISH=false
SKIP_WAIT=false
ANDROID_VERSION=""
IOS_VERSION=""
PLUGIN_VERSION=""

# URLs
MAVEN_BASE_URL="https://repo1.maven.org/maven2/com/newrelic/agent/android/android-agent-static"
IOS_CDN_BASE_URL="https://download.newrelic.com/ios_agent"

# Functions
print_header() {
    echo -e "\n${CYAN}========================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}========================================${NC}\n"
}

print_step() {
    echo -e "${BLUE}→ $1${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${CYAN}ℹ $1${NC}"
}

usage() {
    cat << EOF
Usage: $0 [options]

Options:
    --android-version VERSION   Android agent version (e.g., 7.6.16)
    --ios-version VERSION       iOS agent version (e.g., 7.6.3)
    --plugin-version VERSION    Plugin version (e.g., 1.1.16)
    --dry-run                   Run without making actual changes
    --publish                   Publish packages to NuGet.org
    --skip-android              Skip Android binding release
    --skip-ios                  Skip iOS binding release
    --skip-plugin               Skip main plugin release
    --skip-wait                 Skip waiting for NuGet package indexing
    -h, --help                  Show this help message

Environment Variables:
    NUGET_API_KEY              Required for publishing to NuGet.org

Examples:
    # Dry run with all components
    $0 --android-version 7.6.16 --ios-version 7.6.3 --plugin-version 1.1.16 --dry-run

    # Release Android binding only
    $0 --android-version 7.6.16 --skip-ios --skip-plugin

    # Full release with publishing
    $0 --android-version 7.6.16 --ios-version 7.6.3 --plugin-version 1.1.16 --publish
EOF
    exit 1
}

validate_version() {
    local version=$1
    local name=$2
    if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
        print_error "Invalid $name version format: $version (expected X.X.X)"
        exit 1
    fi
}

check_prerequisites() {
    print_header "Checking Prerequisites"

    # Check for required tools
    local missing_tools=()

    for tool in curl unzip dotnet; do
        if ! command -v "$tool" &> /dev/null; then
            missing_tools+=("$tool")
        fi
    done

    if [ ${#missing_tools[@]} -ne 0 ]; then
        print_error "Missing required tools: ${missing_tools[*]}"
        exit 1
    fi
    print_success "All required tools are available"

    # Check for NUGET_API_KEY if publishing
    if [ "$PUBLISH" = true ] && [ -z "$NUGET_API_KEY" ]; then
        print_error "NUGET_API_KEY environment variable is required for publishing"
        exit 1
    fi

    # Check for uncommitted changes
    if [ -n "$(git -C "$PROJECT_ROOT" status --porcelain)" ]; then
        print_warning "There are uncommitted changes in the repository"
    fi

    print_success "Prerequisites check completed"
}

update_csproj_version() {
    local file=$1
    local version=$2
    local name=$3

    print_step "Updating $name version to $version"

    if [ "$DRY_RUN" = true ]; then
        print_info "[DRY RUN] Would update $file"
        return
    fi

    # Use sed to update the version
    if [[ "$OSTYPE" == "darwin"* ]]; then
        sed -i '' "s|<Version>[^<]*</Version>|<Version>$version</Version>|" "$file"
    else
        sed -i "s|<Version>[^<]*</Version>|<Version>$version</Version>|" "$file"
    fi

    print_success "Updated $name version"
}

update_package_reference() {
    local file=$1
    local package_id=$2
    local version=$3

    print_step "Updating $package_id reference to version $version"

    if [ "$DRY_RUN" = true ]; then
        print_info "[DRY RUN] Would update package reference in $file"
        return
    fi

    if [[ "$OSTYPE" == "darwin"* ]]; then
        sed -i '' "s|<PackageReference Include=\"$package_id\" Version=\"[^\"]*\"|<PackageReference Include=\"$package_id\" Version=\"$version\"|g" "$file"
    else
        sed -i "s|<PackageReference Include=\"$package_id\" Version=\"[^\"]*\"|<PackageReference Include=\"$package_id\" Version=\"$version\"|g" "$file"
    fi

    print_success "Updated $package_id reference"
}

download_android_aar() {
    local version=$1
    local url="${MAVEN_BASE_URL}/${version}/android-agent-static-${version}.aar"
    local output_dir="${PROJECT_ROOT}/NewRelic.MAUI.Android.Binding/Jars"
    local output_file="${output_dir}/android-agent-static-${version}.aar"

    print_header "Downloading Android AAR"
    print_step "URL: $url"

    if [ "$DRY_RUN" = true ]; then
        print_info "[DRY RUN] Would download AAR to $output_file"
        return
    fi

    # Create backup of existing AAR files
    if ls "${output_dir}"/*.aar 1> /dev/null 2>&1; then
        print_step "Backing up existing AAR files"
        mkdir -p "${output_dir}/backup"
        mv "${output_dir}"/*.aar "${output_dir}/backup/" 2>/dev/null || true
    fi

    # Download the AAR
    print_step "Downloading AAR..."
    if curl -fSL -o "$output_file" "$url"; then
        print_success "Downloaded AAR successfully"
    else
        print_error "Failed to download AAR from $url"
        # Restore backup if download failed
        if [ -d "${output_dir}/backup" ]; then
            mv "${output_dir}/backup"/*.aar "${output_dir}/" 2>/dev/null || true
            rm -rf "${output_dir}/backup"
        fi
        exit 1
    fi

    # Clean up backup
    rm -rf "${output_dir}/backup"
}

download_and_process_ios_xcframework() {
    local version=$1
    local url="${IOS_CDN_BASE_URL}/NewRelic_XCFramework_Agent_${version}.zip"
    local temp_dir=$(mktemp -d)
    local output_dir="${PROJECT_ROOT}/NewRelic.MAUI.iOS.Binding"

    print_header "Downloading and Processing iOS XCFramework"
    print_step "URL: $url"

    if [ "$DRY_RUN" = true ]; then
        print_info "[DRY RUN] Would download and process XCFramework"
        return
    fi

    # Backup existing XCFramework
    if [ -d "${output_dir}/NewRelic.xcframework" ]; then
        print_step "Backing up existing XCFramework"
        mv "${output_dir}/NewRelic.xcframework" "${output_dir}/NewRelic.xcframework.backup"
    fi

    # Download the ZIP
    print_step "Downloading XCFramework ZIP..."
    local zip_file="${temp_dir}/NewRelic_XCFramework_Agent_${version}.zip"
    if ! curl -fSL -o "$zip_file" "$url"; then
        print_error "Failed to download XCFramework from $url"
        # Restore backup
        if [ -d "${output_dir}/NewRelic.xcframework.backup" ]; then
            mv "${output_dir}/NewRelic.xcframework.backup" "${output_dir}/NewRelic.xcframework"
        fi
        rm -rf "$temp_dir"
        exit 1
    fi
    print_success "Downloaded XCFramework ZIP"

    # Unzip
    print_step "Extracting XCFramework..."
    unzip -q "$zip_file" -d "$temp_dir"
    print_success "Extracted XCFramework"

    # Find the xcframework directory
    local xcframework_path=$(find "$temp_dir" -name "NewRelic.xcframework" -type d | head -1)
    if [ -z "$xcframework_path" ]; then
        print_error "NewRelic.xcframework not found in ZIP"
        if [ -d "${output_dir}/NewRelic.xcframework.backup" ]; then
            mv "${output_dir}/NewRelic.xcframework.backup" "${output_dir}/NewRelic.xcframework"
        fi
        rm -rf "$temp_dir"
        exit 1
    fi

    # Remove watchOS, tvOS, and maccatalyst folders
    print_step "Removing watchOS, tvOS, and maccatalyst folders..."
    find "$xcframework_path" -type d -name "*watchos*" -exec rm -rf {} + 2>/dev/null || true
    find "$xcframework_path" -type d -name "*tvos*" -exec rm -rf {} + 2>/dev/null || true
    find "$xcframework_path" -type d -name "*maccatalyst*" -exec rm -rf {} + 2>/dev/null || true
    print_success "Removed watchOS, tvOS, and maccatalyst folders"

    # Update Info.plist to remove watchOS and tvOS entries
    print_step "Updating Info.plist..."
    "$SCRIPT_DIR/update-ios-xcframework.sh" "$xcframework_path/Info.plist"
    print_success "Updated Info.plist"

    # Move to output directory
    print_step "Moving XCFramework to project..."
    mv "$xcframework_path" "${output_dir}/NewRelic.xcframework"
    print_success "XCFramework installed"

    # Clean up
    rm -rf "$temp_dir"
    rm -rf "${output_dir}/NewRelic.xcframework.backup"
}

build_package() {
    local project_path=$1
    local name=$2

    print_step "Building $name..."

    if [ "$DRY_RUN" = true ]; then
        print_info "[DRY RUN] Would build $project_path"
        return
    fi

    if dotnet build "$project_path" -c Release; then
        print_success "Built $name successfully"
    else
        print_error "Failed to build $name"
        exit 1
    fi
}

publish_package() {
    local package_dir=$1
    local package_id=$2
    local version=$3

    print_step "Publishing $package_id version $version..."

    if [ "$DRY_RUN" = true ]; then
        print_info "[DRY RUN] Would publish $package_id to NuGet.org"
        return
    fi

    if [ "$PUBLISH" != true ]; then
        print_warning "Skipping publish (use --publish flag to enable)"
        return
    fi

    local nupkg_file=$(find "$package_dir" -name "${package_id}.${version}.nupkg" | head -1)
    if [ -z "$nupkg_file" ]; then
        print_error "NuGet package not found: ${package_id}.${version}.nupkg"
        exit 1
    fi

    if dotnet nuget push "$nupkg_file" --api-key "$NUGET_API_KEY" --source https://api.nuget.org/v3/index.json; then
        print_success "Published $package_id successfully"
    else
        print_error "Failed to publish $package_id"
        exit 1
    fi
}

wait_for_nuget_package() {
    local package_id=$1
    local version=$2
    local timeout=600  # 10 minutes
    local interval=30  # 30 seconds

    if [ "$SKIP_WAIT" = true ]; then
        print_warning "Skipping NuGet package availability check"
        return
    fi

    print_step "Waiting for $package_id version $version to be available on NuGet..."

    if [ "$DRY_RUN" = true ]; then
        print_info "[DRY RUN] Would wait for package availability"
        return
    fi

    local elapsed=0
    local url="https://api.nuget.org/v3-flatcontainer/${package_id,,}/index.json"

    while [ $elapsed -lt $timeout ]; do
        local versions=$(curl -s "$url" 2>/dev/null | grep -o "\"$version\"" || true)
        if [ -n "$versions" ]; then
            print_success "$package_id version $version is now available"
            return
        fi

        print_info "Package not yet available, waiting ${interval}s... (${elapsed}s/${timeout}s)"
        sleep $interval
        elapsed=$((elapsed + interval))
    done

    print_error "Timeout waiting for $package_id to be available"
    exit 1
}

release_android() {
    if [ "$SKIP_ANDROID" = true ]; then
        print_info "Skipping Android binding release"
        return
    fi

    if [ -z "$ANDROID_VERSION" ]; then
        print_error "Android version is required (use --android-version)"
        exit 1
    fi

    print_header "Phase 1: Android Binding Release"

    validate_version "$ANDROID_VERSION" "Android"

    # Calculate binding version (append .1 to distinguish from raw agent version)
    local binding_version="${ANDROID_VERSION}.1"

    # Download AAR
    download_android_aar "$ANDROID_VERSION"

    # Update version in .csproj
    update_csproj_version \
        "${PROJECT_ROOT}/NewRelic.MAUI.Android.Binding/NewRelic.MAUI.Android.Binding.csproj" \
        "$binding_version" \
        "Android binding"

    # Build
    build_package \
        "${PROJECT_ROOT}/NewRelic.MAUI.Android.Binding/NewRelic.MAUI.Android.Binding.csproj" \
        "Android binding"

    # Publish
    publish_package \
        "${PROJECT_ROOT}/NewRelic.MAUI.Android.Binding/bin/Release" \
        "NewRelic.MAUI.Android.Binding" \
        "$binding_version"

    print_success "Android binding release completed"
}

release_ios() {
    if [ "$SKIP_IOS" = true ]; then
        print_info "Skipping iOS binding release"
        return
    fi

    if [ -z "$IOS_VERSION" ]; then
        print_error "iOS version is required (use --ios-version)"
        exit 1
    fi

    print_header "Phase 2: iOS Binding Release"

    validate_version "$IOS_VERSION" "iOS"

    # Download and process XCFramework
    download_and_process_ios_xcframework "$IOS_VERSION"

    # Update version in .csproj
    update_csproj_version \
        "${PROJECT_ROOT}/NewRelic.MAUI.iOS.Binding/NewRelic.MAUI.iOS.Binding.csproj" \
        "$IOS_VERSION" \
        "iOS binding"

    # Build
    build_package \
        "${PROJECT_ROOT}/NewRelic.MAUI.iOS.Binding/NewRelic.MAUI.iOS.Binding.csproj" \
        "iOS binding"

    # Publish
    publish_package \
        "${PROJECT_ROOT}/NewRelic.MAUI.iOS.Binding/bin/Release" \
        "NewRelic.MAUI.iOS.Binding" \
        "$IOS_VERSION"

    print_success "iOS binding release completed"
}

release_plugin() {
    if [ "$SKIP_PLUGIN" = true ]; then
        print_info "Skipping main plugin release"
        return
    fi

    if [ -z "$PLUGIN_VERSION" ]; then
        print_error "Plugin version is required (use --plugin-version)"
        exit 1
    fi

    print_header "Phase 3: Main Plugin Release"

    validate_version "$PLUGIN_VERSION" "Plugin"

    # Calculate binding versions
    local android_binding_version="${ANDROID_VERSION}.1"

    # Wait for binding packages if publishing
    if [ "$PUBLISH" = true ]; then
        if [ "$SKIP_ANDROID" != true ] && [ -n "$ANDROID_VERSION" ]; then
            wait_for_nuget_package "NewRelic.MAUI.Android.Binding" "$android_binding_version"
        fi
        if [ "$SKIP_IOS" != true ] && [ -n "$IOS_VERSION" ]; then
            wait_for_nuget_package "NewRelic.MAUI.iOS.Binding" "$IOS_VERSION"
        fi
    fi

    local plugin_csproj="${PROJECT_ROOT}/NewRelic.MAUI.Plugin/NewRelic.MAUI.Plugin.csproj"

    # Update plugin version
    update_csproj_version "$plugin_csproj" "$PLUGIN_VERSION" "Plugin"

    # Update Android binding reference
    if [ -n "$ANDROID_VERSION" ]; then
        update_package_reference "$plugin_csproj" "NewRelic.MAUI.Android.Binding" "$android_binding_version"
    fi

    # Update iOS binding reference
    if [ -n "$IOS_VERSION" ]; then
        update_package_reference "$plugin_csproj" "NewRelic.MAUI.iOS.Binding" "$IOS_VERSION"
    fi

    # Build
    build_package "$plugin_csproj" "MAUI Plugin"

    # Publish
    publish_package \
        "${PROJECT_ROOT}/NewRelic.MAUI.Plugin/bin/Release" \
        "NewRelic.MAUI.Plugin" \
        "$PLUGIN_VERSION"

    print_success "Plugin release completed"
}

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --android-version)
            ANDROID_VERSION="$2"
            shift 2
            ;;
        --ios-version)
            IOS_VERSION="$2"
            shift 2
            ;;
        --plugin-version)
            PLUGIN_VERSION="$2"
            shift 2
            ;;
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        --publish)
            PUBLISH=true
            shift
            ;;
        --skip-android)
            SKIP_ANDROID=true
            shift
            ;;
        --skip-ios)
            SKIP_IOS=true
            shift
            ;;
        --skip-plugin)
            SKIP_PLUGIN=true
            shift
            ;;
        --skip-wait)
            SKIP_WAIT=true
            shift
            ;;
        -h|--help)
            usage
            ;;
        *)
            print_error "Unknown option: $1"
            usage
            ;;
    esac
done

# Main execution
print_header "NewRelic MAUI Plugin Release"

if [ "$DRY_RUN" = true ]; then
    print_warning "Running in DRY RUN mode - no changes will be made"
fi

echo "Configuration:"
echo "  Android Version: ${ANDROID_VERSION:-'(not set)'}"
echo "  iOS Version: ${IOS_VERSION:-'(not set)'}"
echo "  Plugin Version: ${PLUGIN_VERSION:-'(not set)'}"
echo "  Dry Run: $DRY_RUN"
echo "  Publish: $PUBLISH"
echo "  Skip Android: $SKIP_ANDROID"
echo "  Skip iOS: $SKIP_IOS"
echo "  Skip Plugin: $SKIP_PLUGIN"

check_prerequisites

# Run release phases
release_android
release_ios
release_plugin

print_header "Release Complete"
print_success "All release phases completed successfully!"

if [ "$DRY_RUN" = true ]; then
    print_info "This was a dry run. Re-run without --dry-run to make actual changes."
fi

if [ "$PUBLISH" != true ]; then
    print_info "Packages were not published. Use --publish flag to publish to NuGet.org"
fi
