$ErrorActionPreference = "Stop"

$SolutionRoot = $PSScriptRoot
$Project = Join-Path $SolutionRoot "src/UltimaIII.Avalonia/UltimaIII.Avalonia.csproj"
$BuildsDir = Join-Path $SolutionRoot "BUILDS"

$Targets = @{
    "win-x64"     = "WINDOWSx86"
    "win-arm64"   = "WINDOWSarm"
    "osx-x64"     = "MACOSx86"
    "osx-arm64"   = "MACOSarm"
    "linux-x64"   = "LINUXx86"
    "linux-arm64"  = "LINUXarm"
}

Write-Host "========================================"
Write-Host "  AVAUltima - Publish Build Script"
Write-Host "========================================"
Write-Host ""

# Clean BUILDS directory
if (Test-Path $BuildsDir) {
    Write-Host "Cleaning previous builds..."
    Remove-Item -Recurse -Force $BuildsDir
}
New-Item -ItemType Directory -Path $BuildsDir | Out-Null

$Failed = @()

foreach ($RID in $Targets.Keys) {
    $Folder = $Targets[$RID]
    $Output = Join-Path $BuildsDir $Folder

    Write-Host ""
    Write-Host "--- Building $Folder ($RID) ---"

    dotnet publish $Project `
        --configuration Release `
        --runtime $RID `
        --self-contained true `
        --output $Output `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:DebugType=none `
        -p:DebugSymbols=false

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  -> $Folder OK"
    } else {
        Write-Host "  -> $Folder FAILED"
        $Failed += $Folder
    }
}

Write-Host ""
Write-Host "========================================"
Write-Host "  Build Summary"
Write-Host "========================================"

foreach ($RID in $Targets.Keys) {
    $Folder = $Targets[$RID]
    $Output = Join-Path $BuildsDir $Folder

    if ((Test-Path $Output) -and (Get-ChildItem $Output -ErrorAction SilentlyContinue)) {
        $Size = "{0:N1} MB" -f ((Get-ChildItem $Output -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB)
        Write-Host "  $Folder  ->  $Size"
    } else {
        Write-Host "  $Folder  ->  MISSING"
    }
}

if ($Failed.Count -gt 0) {
    Write-Host ""
    Write-Host "FAILED: $($Failed -join ', ')"
    exit 1
} else {
    Write-Host ""
    Write-Host "All builds completed successfully!"
    Write-Host "Output: $BuildsDir"
}
