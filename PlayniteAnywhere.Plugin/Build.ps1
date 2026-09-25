$ErrorActionPreference = "Stop"

$ProjectDir = $PSScriptRoot

$ProjectFile = Join-Path $ProjectDir "PlayniteAnywhere.csproj"
$Toolbox = Join-Path $env:LOCALAPPDATA "Playnite\Toolbox.exe"


$BinDir = Join-Path $ProjectDir "bin"
$ReleaseDir = Join-Path $BinDir "Release"
$SubReleaseDir = Join-Path $ReleaseDir "net462"


$PackageDir = Join-Path $BinDir "Package"


# ----------------------------------------------------------------------
# Vérifications
# ----------------------------------------------------------------------

if (-not (Test-Path $Toolbox)) {
    throw "Playnite Toolbox introuvable : $Toolbox"
}

if (-not (Test-Path $ProjectFile)) {
    throw "Projet introuvable : $ProjectFile"
}

if (-not (Test-Path (Join-Path $ProjectDir "extension.yaml"))) {
    throw "extension.yaml introuvable."
}




# ----------------------------------------------------------------------
# Nettoyage
# ----------------------------------------------------------------------

Write-Host ""
Write-Host "=== Nettoyage ==="

Remove-Item $PackageDir -Recurse -Force -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Path $PackageDir -Force | Out-Null

# On conserve bin\Release pour la sortie finale.
New-Item -ItemType Directory -Path $ReleaseDir -Force | Out-Null

# Supprime uniquement les anciens packages.
Get-ChildItem $ReleaseDir -File |
    Where-Object {
        $_.Extension -in ".pext", ".pthm"
    } |
    Remove-Item -Force


# ----------------------------------------------------------------------
# Compilation
# ----------------------------------------------------------------------

Write-Host ""
Write-Host "=== Compilation ==="

dotnet build `
    $ProjectFile `
    /property:GenerateFullPaths=true `
    /p:Configuration=Release `
    /p:Platform=AnyCPU `
    /consoleloggerparameters:NoSummary

if ($LASTEXITCODE -ne 0) {
    throw "La compilation a échoué."
}


# ----------------------------------------------------------------------
# Préparation du package extension
# ----------------------------------------------------------------------

Write-Host ""
Write-Host "=== Préparation de l'extension ==="


Copy-Item (Join-Path $SubReleaseDir "extension.yaml") $PackageDir -Force
Copy-Item (Join-Path $SubReleaseDir "Web") $PackageDir -Recurse -Force

#$ExtensionManifest = Get-Content `
#    (Join-Path $ProjectDir "extension.yaml") `
#    -Raw
#$ExtensionManifest |
#    Set-Content `
#        (Join-Path $PackageDir "extension.yaml") `
#        -Encoding UTF8

$DllPath = Join-Path $SubReleaseDir "PlayniteAnywhere.dll"
if (-not (Test-Path $DllPath)) {
    throw "DLL compilée introuvable : $DllPath"
}
Copy-Item $DllPath $PackageDir -Force
$DllPath = Join-Path $SubReleaseDir "PlayniteAnywhere.Common.dll"
if (-not (Test-Path $DllPath)) {
    throw "DLL compilée introuvable : $DllPath"
}
Copy-Item $DllPath $PackageDir -Force
    
$DllPath = Join-Path $SubReleaseDir "EmbedIO.dll"
if (-not (Test-Path $DllPath)) {
    throw "DLL EmbedIO introuvable : $DllPath"
}
Copy-Item $DllPath $PackageDir -Force
    
$DllPath = Join-Path $SubReleaseDir "Swan.Lite.dll"
if (-not (Test-Path $DllPath)) {
    throw "DLL Swan.Lite introuvable : $DllPath"
}
Copy-Item $DllPath $PackageDir -Force




# ----------------------------------------------------------------------
# Packaging .pext
# ----------------------------------------------------------------------

Write-Host ""
Write-Host "=== Packaging .pext ==="

& $Toolbox pack `
    $PackageDir `
    $ReleaseDir

if ($LASTEXITCODE -ne 0) {
    throw "Le packaging de l'extension a échoué."
}



# ----------------------------------------------------------------------
# Nettoyage du staging
# ----------------------------------------------------------------------

Remove-Item $PackageDir -Recurse -Force


# ----------------------------------------------------------------------
# Résultat
# ----------------------------------------------------------------------

Write-Host ""
Write-Host "========================================"
Write-Host " BUILD TERMINE"
Write-Host "========================================"
Write-Host ""
Write-Host "Packages générés dans :"
Write-Host "  $ReleaseDir"
Write-Host ""

$Packages = Get-ChildItem $ReleaseDir -File |
    Where-Object {
        $_.Extension -in ".pext", ".pthm"
    }

if ($Packages.Count -eq 0) {
    throw "Aucun package n'a été généré."
}

foreach ($Package in $Packages) {
    Write-Host "  $($Package.Name)"
}

Write-Host ""