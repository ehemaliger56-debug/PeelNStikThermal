param([string]$Version = '1.0.0')

$ErrorActionPreference = 'Stop'

$modRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$stage   = Join-Path ([System.IO.Path]::GetTempPath()) ('chs-release-' + [guid]::NewGuid().ToString('N'))
$target  = Join-Path $stage 'ConformalHeatShields'

New-Item -ItemType Directory -Path $target -Force | Out-Null
Get-ChildItem -Path $modRoot -Force | Copy-Item -Destination $target -Recurse -Force

$zip = Join-Path $PSScriptRoot "ConformalHeatShields-$Version.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path $target -DestinationPath $zip -CompressionLevel Optimal
Remove-Item $stage -Recurse -Force

Write-Host "Created $zip"
