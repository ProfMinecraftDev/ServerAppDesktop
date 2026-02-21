[CmdletBinding()]
param (
    [Parameter()]
    [switch]$NoPause
)

TASKKILL.exe /F /IM "devenv.exe" /T

Set-Location -Path ..
".vs", "bin", "obj", "*.user", "export" | ForEach-Object { 
    Get-ChildItem -Path . -Include $_ -Recurse -Force -ErrorAction SilentlyContinue | 
    Remove-Item -Recurse -Force -Confirm:$false 
}

Write-Host "Proyecto purgado." -ForegroundColor Magenta

if (-not $NoPause){
    Write-Host "`nPresiona Enter para continuar..." -ForegroundColor Gray
    Read-Host
}