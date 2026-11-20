param([string]$Version = '1.0.0')

$ErrorActionPreference = 'Stop'

$modRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$stage   = Join-Path ([System.IO.Path]::GetTempPath()) ('chs-release-' + [guid]::NewGuid().ToString('N'))
$target  = Join-Path $stage 'PeelNStikThermal'

New-Item -ItemType Directory -Path $target -Force | Out-Null
& robocopy $modRoot $target /E /XD .git /XF *.zip *.log /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "robocopy failed with exit code $LASTEXITCODE" }

$zip = Join-Path $PSScriptRoot "PeelNStikThermal-$Version.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path $target -DestinationPath $zip -CompressionLevel Optimal
Remove-Item $stage -Recurse -Force

Write-Host "Created $zip"
