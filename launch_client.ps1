$exePath = Join-Path $env:VINTAGE_STORY "Vintagestory.exe"
$dataPath = Join-Path $env:appdata "VintagestoryData"

$cwd = Get-Location
$modPath = Join-Path $cwd "BathTime/bin/Debug/Mods"

Write-Host $modPath
Write-Host $exePath
Write-Host $dataPath

& $exePath  --dataPath $dataPath --addModPath $modPath
