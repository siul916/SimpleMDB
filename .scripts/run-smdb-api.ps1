param(
    [string]$ProjectPath = "src\Smdb.Api\Smdb.Api.csproj",
    [string]$Configuration = "Debug"
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir "..")
$projectFull = Resolve-Path (Join-Path $repoRoot $ProjectPath)
$publishDir = Join-Path $repoRoot "_published\Smdb.Api"

Write-Host "Project: $projectFull"
Write-Host "Publish dir: $publishDir"

# Kill any running instance that was started from the same publish dir
Get-Process -ErrorAction SilentlyContinue | Where-Object {
    try {
        ($_.Path -and ($_.Path -like "*dotnet.exe")) -and ($_.CommandLine -like "*$publishDir*")
    } catch { $false }
} | ForEach-Object {
    Write-Host "Stopping process PID=$($_.Id) CommandLine=$($_.CommandLine)"
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}

# Clean publish dir
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
New-Item -ItemType Directory -Path $publishDir | Out-Null

# Publish the project to the publish dir
Write-Host "Publishing..."
dotnet publish $projectFull -c $Configuration -o $publishDir
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet publish failed with exit code $LASTEXITCODE"; exit $LASTEXITCODE }

# Find the published dll
$dll = Get-ChildItem -Path $publishDir -Filter "*.dll" | Where-Object { $_.Name -like "Smdb.Api*.dll" } | Select-Object -First 1
if (-not $dll) {
    $dll = Get-ChildItem -Path $publishDir -Filter "Smdb.Api.dll" | Select-Object -First 1
}
if (-not $dll) { Write-Error "Could not find published dll in $publishDir"; exit 1 }

# Start the app
Write-Host "Starting: dotnet $($dll.FullName)"
$proc = Start-Process -FilePath "dotnet" -ArgumentList @($dll.FullName) -PassThru
Write-Host "Started PID=$($proc.Id)"

# Output some run info
Write-Host "To stop: Stop-Process -Id $($proc.Id)"

exit 0
