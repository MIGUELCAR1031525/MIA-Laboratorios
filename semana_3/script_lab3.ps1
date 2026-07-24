# Solicitar el nombre del estudiante
$nombre = Read-Host "Ingrese su nombre completo"
# Obtener la fecha y hora actual
$fecha = Get-Date
# Crear las carpetas
New-Item -ItemType Directory -Force -Path "Datos","Respaldo"
# Crear un archivo personalizado
"Nombre del estudiante: $nombre" | Out-File "Datos\$nombre.txt"
"Fecha de creación: $fecha" | Add-Content "Datos\$nombre.txt"
"Laboratorio 3 - Creación de Scripts en PowerShell y Bash" | Add-Content "Datos\$nombre.txt"
# Crear una copia de respaldo
Copy-Item "Datos\$nombre.txt" "Respaldo\$nombre`_backup.txt"
# Mostrar la estructura de carpetas
Write-Host "`n=== Estructura de directorios ==="
tree
# Mostrar todos los archivos creados
Write-Host "`n=== Archivos creados ==="
Get-ChildItem -Recurse
# Mostrar el contenido del archivo
Write-Host "`n=== Contenido del archivo ==="
Get-Content "Datos\$nombre.txt"
