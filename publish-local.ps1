<#
.SYNOPSIS
    Baut avUpload lokal (Release|AnyCPU) sauber neu inkl. Signieren und Setup.

.DESCRIPTION
    Entspricht einem normalen Release-Build in Visual Studio: msbuild baut
    avUpload.sln in der Konfiguration Release. Der PostBuildEvent in
    avUpload.csproj signiert danach automatisch die EXE (sign.cmd) und
    erzeugt das Setup (iscc InstallScript.iss) - hier ist dafuer kein
    eigener Schritt noetig.

    avUpload ist ein klassisches (Non-SDK) .NET-Framework-Projekt (net48) mit
    packages.config statt PackageReference. Daher reicht "msbuild" allein
    nicht wie bei "dotnet publish" - die NuGet-Pakete muessen vorher per
    "nuget restore" geholt werden.

    bin und obj werden vor dem Bauen komplett geloescht. Das ist hier bewusst
    kein Default-Overridable: Bei den letzten beiden Testlaeufen wurde
    wiederholt eine veraltete avUpload.exe getestet (einmal vor Einfuehrung
    des Silent-Modus, einmal vor dem SSH.NET-2026.0.0-Upgrade), weil nur die
    EXE neu gebaut/kopiert, nicht aber sauber neu gebaut wurde. Ein
    garantiert frischer Build ist hier wichtiger als die paar Sekunden
    inkrementeller Build-Zeit.

.EXAMPLE
    .\publish-local.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

# Ausgaben auf Englisch (VSLANG, nicht DOTNET_CLI_UI_LANGUAGE - hier kommt
# klassisches msbuild.exe zum Einsatz) - leichter nachschlagbare
# Fehlermeldungen.
$env:VSLANG = '1033'

# --- Werkzeuge suchen --------------------------------------------------------
function Find-MSBuild {
    $cmd = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $path = & $vswhere -latest -prerelease -requires Microsoft.Component.MSBuild `
            -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
        if ($path) { return $path }
    }

    throw "MSBuild nicht gefunden. Entweder in einer 'Developer PowerShell for VS' " +
          "ausfuehren oder Visual Studio (mit .NET-Desktopentwicklung) installieren."
}

function Find-NuGet {
    $cmd = Get-Command nuget.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -property installationPath
        if ($vsPath) {
            $bundled = Join-Path $vsPath 'Common7\IDE\CommonExtensions\Microsoft\NuGet\NuGet.exe'
            if (Test-Path $bundled) { return $bundled }
        }
    }

    throw "nuget.exe nicht gefunden. Installieren mit:`n" +
          "  winget install --id Microsoft.NuGet`n" +
          "Danach PowerShell neu starten (PATH)."
}

$msbuild = Find-MSBuild
$nuget   = Find-NuGet

# --- Sauber neu bauen ---------------------------------------------------------
foreach ($dir in @('bin', 'obj')) {
    if (Test-Path $dir) {
        Write-Host "Entferne $dir" -ForegroundColor DarkGray
        Remove-Item $dir -Recurse -Force
    }
}

Write-Host "`nStelle NuGet-Pakete wieder her..." -ForegroundColor Cyan
& $nuget restore 'avUpload.sln'
if ($LASTEXITCODE -ne 0) {
    throw "nuget restore ist mit Code $LASTEXITCODE fehlgeschlagen."
}

Write-Host "`nBaue avUpload (Release)..." -ForegroundColor Cyan
& $msbuild 'avUpload.sln' /p:Configuration=Release /v:minimal /m

# WICHTIG: PowerShell wertet Rueckgabewerte externer Programme nicht als Fehler
# aus - $ErrorActionPreference = 'Stop' greift bei msbuild und iscc nicht.
if ($LASTEXITCODE -ne 0) {
    throw "msbuild ist mit Code $LASTEXITCODE fehlgeschlagen. Fehlermeldungen stehen oben."
}

$exe = 'bin\Release\avUpload.exe'
if (-not (Test-Path $exe)) {
    throw "Kompilierung lief durch, aber $exe fehlt - bitte oben pruefen."
}

$exeVersion = (Get-Item $exe).VersionInfo.FileVersion
Write-Host "`nFertig: $exe (Version $exeVersion)" -ForegroundColor Green

$setup = Get-ChildItem 'bin\Release\avUpload_setup-*.exe' -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($setup) {
    Write-Host "Setup erstellt: $($setup.FullName)" -ForegroundColor Green
}
else {
    Write-Host "Kein Setup gefunden - vermutlich ist iscc nicht gelaufen (siehe PostBuildEvent in avUpload.csproj)." -ForegroundColor Yellow
}
