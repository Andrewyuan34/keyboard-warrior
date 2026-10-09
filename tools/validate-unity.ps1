param(
    [ValidateSet('All', 'EditMode', 'PlayMode', 'Build', 'Smoke')]
    [string]$Mode = 'All',
    [string]$EditorPath
)

$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
$project = Join-Path $repository 'Game'
$results = Join-Path $repository 'artifacts\validation'
$player = Join-Path $repository 'Builds\Windows\Validation\KeyboardWarrior.exe'
$version = ((Get-Content "$project\ProjectSettings\ProjectVersion.txt" | Select-Object -First 1) -split ': ')[1]
if (-not $EditorPath) { $EditorPath = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe" }
if ($Mode -ne 'Smoke' -and -not (Test-Path -LiteralPath $EditorPath)) { throw "Editor not found: $EditorPath" }
New-Item -ItemType Directory -Path $results -Force | Out-Null

foreach ($platform in @('EditMode', 'PlayMode')) {
    if ($Mode -ne 'All' -and $Mode -ne $platform) { continue }
    $xmlPath = Join-Path $results "$platform.xml"
    $logPath = Join-Path $results "$platform.log"
    if (Test-Path -LiteralPath $xmlPath) { Remove-Item -LiteralPath $xmlPath }
    $arguments = @('-batchmode', '-projectPath', $project, '-runTests', '-testPlatform', $platform,
        '-testFilter', 'KeyboardWarrior.Validation.Tests', '-testResults', $xmlPath, '-logFile', $logPath)
    & $EditorPath @arguments | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "$platform failed. See $logPath" }
    [xml]$report = Get-Content -LiteralPath $xmlPath
    $runResult = $report.'test-run'
    if ($runResult.result -ne 'Passed' -or [int]$runResult.total -eq 0 -or
        [int]$runResult.passed -ne [int]$runResult.total) { throw "Incomplete or failing results: $xmlPath" }
    Write-Output "$platform : $($report.'test-run'.passed)/$($report.'test-run'.total) passed"
}

if ($Mode -eq 'All' -or $Mode -eq 'Build') {
    $arguments = @('-batchmode', '-quit', '-projectPath', $project,
        '-executeMethod', 'KeyboardWarrior.Development.DevelopmentTasks.BuildWindows',
        '-logFile', (Join-Path $results 'build.log'))
    & $EditorPath @arguments | Out-Null
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $player)) { throw "Windows build failed. See $results\build.log" }
    Write-Output "Windows build: $player"
}

if ($Mode -eq 'All' -or $Mode -eq 'Smoke') {
    foreach ($fps in @(30, 60, 120)) {
        $jsonPath = Join-Path $results "standalone-$fps.json"
        $logPath = Join-Path $results "standalone-$fps.log"
        if (Test-Path -LiteralPath $jsonPath) { Remove-Item -LiteralPath $jsonPath }
        $arguments = "-validationSmoke `"$jsonPath`" -validationFps $fps -logFile `"$logPath`""
        # A visible swap chain is required for real framebuffer capture on this Windows setup.
        $run = Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Normal -PassThru
        if (-not $run.WaitForExit(180000)) { $run.Kill(); throw "Standalone $fps FPS timed out." }
        if ($run.ExitCode -ne 0) { throw "Standalone $fps FPS failed. See $jsonPath and $logPath" }
        $report = Get-Content -LiteralPath $jsonPath -Raw | ConvertFrom-Json
        if (-not $report.passed -or $report.cases.Count -eq 0) { throw "Invalid or failing report: $jsonPath" }
        Write-Output "Standalone $fps FPS: $($report.cases.Count) checks passed"
    }
}
