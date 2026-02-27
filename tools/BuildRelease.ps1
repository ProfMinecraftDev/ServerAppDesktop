$cleanSolutionPath = Join-Path "$PSScriptRoot" "CleanSolution.ps1"

& $cleanSolutionPath -NoPause
dotnet publish ServerAppDesktop.sln /nologo /p:Configuration=Release -v m

$PossiblePaths = @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
)

$InnoSetupPath = $null

foreach ($Path in $PossiblePaths) {
    if (Test-Path $Path) {
        $InnoSetupPath = $Path
        break
    }
}

New-Item -ItemType Directory -Path "export\installer" -Force

if ($InnoSetupPath) {
    & $InnoSetupPath "installer\Script_x64.iss"
} else {
    Write-Host "¡Error! Inno Setup no está instalado en ninguna ruta conocida." -ForegroundColor Red
    exit
}

Read-Host