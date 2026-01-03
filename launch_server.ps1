$exePath = Join-Path $env:VINTAGE_STORY "VintagestoryServer.exe"
$dataPath = Join-Path $env:appdata "VintagestoryData_Server"

$cwd = Get-Location
$modPath = Join-Path $cwd "BathTime/bin/Debug/Mods"

Write-Host $modPath
Write-Host $exePath
Write-Host $dataPath

& $exePath  --dataPath $dataPath --addModPath $modPath
