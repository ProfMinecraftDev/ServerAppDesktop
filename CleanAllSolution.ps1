TASKKILL.exe /IM "devenv.exe" /F /T
Write-Host "Eliminando carpeta .vs..." -ForegroundColor Cyan
try {
    if (Test-Path ".vs") {
        Remove-Item ".vs" -Recurse -Force
        Write-Host "Carpeta .vs eliminada. No dejó ni rastro." -ForegroundColor Green
    } else {
        Write-Host "La carpeta .vs no existe. O se escondió, o nunca estuvo." -ForegroundColor Yellow
    }
} catch {
    Write-Host "Error al eliminar .vs. Se defendió con uñas y dientes." -ForegroundColor Red
}

Write-Host "Buscando carpetas bin y obj..." -ForegroundColor Cyan
try {
    $folders = Get-ChildItem -Path . -Include bin,obj -Recurse -Directory
    if ($folders.Count -eq 0) {
        Write-Host "No se encontraron carpetas bin ni obj. O está limpio, o está vacío por accidente." -ForegroundColor Yellow
    } else {
        foreach ($folder in $folders) {
            try {
                Remove-Item $folder.FullName -Recurse -Force
                Write-Host "$($folder.FullName) eliminada. Justicia técnica ejecutada." -ForegroundColor Green
            } catch {
                Write-Host "No se pudo eliminar $($folder.FullName). Se resistió hasta el final." -ForegroundColor Red
            }
        }
    }
} catch {
    Write-Host "Error al buscar carpetas. El caos reina." -ForegroundColor Red
}

Write-Host "Buscando archivos .user..." -ForegroundColor Cyan
try {
    $files = Get-ChildItem -Path . -Include *.user -Recurse -File
    if ($files.Count -eq 0) {
        Write-Host "No se encontraron archivos .user. O nunca existieron, o ya huyeron." -ForegroundColor Yellow
    } else {
        foreach ($file in $files) {
            try {
                Remove-Item $file.FullName -Force
                Write-Host "$($file.FullName) eliminado. Adiós configuraciones personales." -ForegroundColor Green
            } catch {
                Write-Host "No se pudo eliminar $($file.FullName). Se aferró a la vida." -ForegroundColor Red
            }
        }
    }
} catch {
    Write-Host "Error al buscar archivos .user. El misterio persiste." -ForegroundColor Red
}

Write-Host "Listo. El proyecto está limpio. Por ahora." -ForegroundColor Magenta

Pause