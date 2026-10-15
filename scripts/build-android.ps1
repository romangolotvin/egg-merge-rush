# Build Egg Merge Rush APK via Unity batchmode.
# Usage: powershell -ExecutionPolicy Bypass -File scripts\build-android.ps1

$ErrorActionPreference = "Stop"
$Project = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$OutApk = Join-Path $Project "Builds\Android\EggMergeRush.apk"
$Log = Join-Path $Project "Builds\android-batch.log"

$candidates = @(
    "C:\Users\Roman\Unity\Hub\Editor\6000.0.85f1\Editor\Unity.exe",
    "C:\Program Files\Unity\Hub\Editor\6000.0.85f1\Editor\Unity.exe"
) + @(
    Get-ChildItem "C:\Users\Roman\Unity\Hub\Editor","C:\Program Files\Unity\Hub\Editor" -Filter "Unity.exe" -Recurse -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty FullName
)

$Unity = $candidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
if (-not $Unity) {
    throw "Unity.exe not found. Install Unity 6000.0.85f1 + Android modules first."
}

New-Item -ItemType Directory -Force -Path (Split-Path $OutApk) | Out-Null
Write-Host "Unity: $Unity"
Write-Host "Project: $Project"
Write-Host "Out: $OutApk"

& $Unity `
    -batchmode `
    -nographics `
    -quit `
    -projectPath $Project `
    -executeMethod EggMerge.EditorTools.AndroidBuild.BuildApk `
    -logFile $Log

if ($LASTEXITCODE -ne 0) {
    Write-Host "--- log tail ---"
    if (Test-Path $Log) { Get-Content $Log -Tail 60 }
    throw "Unity batch build failed with exit $LASTEXITCODE"
}

if (-not (Test-Path $OutApk)) {
    Write-Host "--- log tail ---"
    if (Test-Path $Log) { Get-Content $Log -Tail 80 }
    throw "APK not created: $OutApk"
}

Get-Item $OutApk | Format-List FullName, Length, LastWriteTime
Write-Host "OK"
