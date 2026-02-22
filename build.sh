#!/usr/bin/env zsh
set -euo pipefail

SOLUTION_ROOT="$(cd "$(dirname "$0")" && pwd)"
PROJECT="$SOLUTION_ROOT/src/UltimaIII.Avalonia/UltimaIII.Avalonia.csproj"
BUILDS_DIR="$SOLUTION_ROOT/BUILDS"

# Each entry: "RID|FolderName"
TARGETS=(
    "win-x64|WINDOWSx86"
    "win-arm64|WINDOWSarm"
    "osx-x64|MACOSx86"
    "osx-arm64|MACOSarm"
    "linux-x64|LINUXx86"
    "linux-arm64|LINUXarm"
)

echo "========================================"
echo "  AVAUltima - Publish Build Script"
echo "========================================"
echo ""

# Clean BUILDS directory
if [ -d "$BUILDS_DIR" ]; then
    echo "Cleaning previous builds..."
    rm -rf "$BUILDS_DIR"
fi
mkdir -p "$BUILDS_DIR"

FAILED=()

for ENTRY in "${TARGETS[@]}"; do
    RID="${ENTRY%%|*}"
    FOLDER="${ENTRY##*|}"
    OUTPUT="$BUILDS_DIR/$FOLDER"

    echo ""
    echo "--- Building $FOLDER ($RID) ---"

    if dotnet publish "$PROJECT" \
        --configuration Release \
        --runtime "$RID" \
        --self-contained true \
        --output "$OUTPUT" \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:DebugType=none \
        -p:DebugSymbols=false \
        2>&1 | tail -5; then
        echo "  -> $FOLDER OK"
    else
        echo "  -> $FOLDER FAILED"
        FAILED+=("$FOLDER")
    fi
done

echo ""
echo "========================================"
echo "  Build Summary"
echo "========================================"

for ENTRY in "${TARGETS[@]}"; do
    FOLDER="${ENTRY##*|}"
    OUTPUT="$BUILDS_DIR/$FOLDER"
    if [ -d "$OUTPUT" ] && [ "$(ls -A "$OUTPUT" 2>/dev/null)" ]; then
        SIZE=$(du -sh "$OUTPUT" | cut -f1)
        echo "  $FOLDER  ->  $SIZE"
    else
        echo "  $FOLDER  ->  MISSING"
    fi
done

if [ ${#FAILED[@]} -gt 0 ]; then
    echo ""
    echo "FAILED: ${FAILED[*]}"
    exit 1
else
    echo ""
    echo "All builds completed successfully!"
    echo "Output: $BUILDS_DIR/"
fi
