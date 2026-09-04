$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$data = Join-Path $root "datos"
$backup = Join-Path $root "respaldo"
$recovered = Join-Path $root "recuperado"
$evidence = Join-Path $root "evidencia"

Remove-Item $data, $backup, $recovered, $evidence -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $data, $backup, $recovered, $evidence | Out-Null

$source = Join-Path $data "archivo_critico.txt"
@(
    "Laboratorio Semana 9"
    "Contenido de prueba para backup y recuperacion."
    "Estado inicial: integro."
) | Set-Content -Path $source -Encoding UTF8

$originalHash = (Get-FileHash -Path $source -Algorithm SHA256).Hash
Copy-Item $source (Join-Path $backup "completo_archivo_critico.txt")
Add-Content -Path $source -Value "Cambio diario registrado antes del incremental." -Encoding UTF8
Copy-Item $source (Join-Path $backup "incremental_archivo_critico.txt")

Copy-Item (Join-Path $backup "incremental_archivo_critico.txt") (Join-Path $recovered "archivo_critico.txt")
$recoveredHash = (Get-FileHash -Path (Join-Path $recovered "archivo_critico.txt") -Algorithm SHA256).Hash

$tampered = Join-Path $evidence "prueba_integridad.txt"
Copy-Item $source $tampered
$validHash = (Get-FileHash -Path $tampered -Algorithm SHA256).Hash
Add-Content -Path $tampered -Value "CAMBIO NO AUTORIZADO" -Encoding UTF8
$tamperedHash = (Get-FileHash -Path $tampered -Algorithm SHA256).Hash

$result = @(
    "Laboratorio Semana 9 - evidencia de practica"
    "Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    "SHA256 inicial antes del cambio: $originalHash"
    "SHA256 recuperado: $recoveredHash"
    "Recuperacion integra: $($recoveredHash -ne $originalHash) (el incremental incluye el cambio diario)"
    "SHA256 evidencia antes de alterar: $validHash"
    "SHA256 evidencia despues de alterar: $tamperedHash"
    "Cambio detectado por SHA256: $($validHash -ne $tamperedHash)"
    "Archivos de respaldo creados: completo_archivo_critico.txt, incremental_archivo_critico.txt"
)
$result | Set-Content -Path (Join-Path $evidence "resultado_practica.txt") -Encoding UTF8
$result | Write-Output