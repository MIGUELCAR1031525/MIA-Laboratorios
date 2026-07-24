#!/bin/bash
# Solicitar el nombre del estudiante
read -p "Ingrese su nombre completo: " nombre
# Obtener la fecha y hora actual
fecha=$(date)
# Crear las carpetas
mkdir -p Datos Respaldo
# Crear un archivo personalizado
echo "Nombre del estudiante: $nombre" > "Datos/$nombre.txt"
echo "Fecha de creación: $fecha" >> "Datos/$nombre.txt"
echo "Laboratorio 3 - Creación de Scripts en PowerShell y Bash" >> "Datos/$nombre.txt"
# Crear una copia de respaldo
cp "Datos/$nombre.txt" "Respaldo/${nombre}_backup.txt"
# Mostrar la estructura de carpetas
echo
echo "=== Estructura de directorios ==="
tree
# Mostrar todos los archivos creados
echo
echo "=== Archivos creados ==="
find .
# Mostrar el contenido del archivo
echo
echo "=== Contenido del archivo ==="
cat "Datos/$nombre.txt
