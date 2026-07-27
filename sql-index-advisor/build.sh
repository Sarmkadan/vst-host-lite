#!/usr/bin/env bash
# Build script for the vst-host-lite repository.
# This script is invoked by the automated build command and should exit with a non‑zero
# status if the build fails.

# Determine the repository root (assumed to be the sibling directory named "vst-host-lite").
REPO_ROOT="$(cd "$(dirname "$0")/../vst-host-lite" && pwd)"

# Change to the repository root.
cd "$REPO_ROOT" || {
    echo "Failed to change directory to $REPO_ROOT"
    exit 1
}

# Run the .NET build. Use the Release configuration by default.
dotnet build --configuration Release
BUILD_EXIT_CODE=$?

if [ $BUILD_EXIT_CODE -ne 0 ]; then
    echo "dotnet build failed with exit code $BUILD_EXIT_CODE"
    exit $BUILD_EXIT_CODE
fi

echo "Build succeeded."
exit 0
