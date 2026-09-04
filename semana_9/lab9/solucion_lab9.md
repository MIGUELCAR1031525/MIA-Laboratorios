# Laboratorio Semana 9 - Acceso, formatos, integridad y respaldos

**Estudiante:** Miguel

## Objetivo

Aplicar conceptos de acceso, indexacion, formatos, integridad y respaldos en situaciones reales.

## 1. Busqueda

Para una tienda con 300,000 productos recomendaria acceso directo mediante un indice tipo B-tree sobre el codigo del producto. El codigo debe ser unico y estar indexado.

Una busqueda por igualdad (`codigo = P-105832`) no tendria que recorrer los 300,000 registros: el indice localiza el rango o la hoja correspondiente y luego se accede al registro. El costo esperado es aproximadamente O(log n), en lugar de O(n) de una busqueda secuencial. Si los registros estan en un archivo, el indice puede guardar pares codigo-posicion para saltar directamente al registro.

## 2. Indice invertido

Un indice invertido relaciona cada palabra con la lista de documentos en los que aparece. Primero se normaliza el texto, se separan las palabras y se guarda una entrada por termino.

Ejemplo:

```text
seguridad -> [doc01, doc04, doc09]
archivo   -> [doc02, doc04]
respaldo  -> [doc01, doc02, doc09]
```

Para una consulta como `seguridad AND respaldo`, el sistema intersecta las listas y devuelve `[doc01, doc09]`. Asi evita leer todos los documentos en cada busqueda.

## 3. Formatos

| Situacion | Formato | Justificacion |
|---|---|---|
| Enviar datos a una aplicacion web | JSON | Es compacto, legible y representa objetos y arreglos directamente; es compatible con APIs y JavaScript. |
| Compartir una tabla de ventas | CSV | Representa filas y columnas con simplicidad y puede abrirse en hojas de calculo. |
| Almacenar informacion jerarquica | XML | Permite anidar elementos, usar atributos y validar una estructura mediante un esquema. |

TSV tambien es apropiado para tablas cuando los datos contienen muchas comas. No lo elegiria como primera opcion aqui porque CSV es mas comun para intercambio de ventas.

## 4. Integridad

Si la huella SHA-256 cambia despues de copiar el archivo, el archivo copiado no es identico al original. Puede haberse corrompido durante la copia, haberse truncado, haberse modificado o provenir de una fuente incorrecta. La huella no explica la causa, pero si evidencia que el contenido cambio.

Antes de utilizarlo se debe comparar la huella calculada del destino con la huella oficial del origen, verificar el tamano, confirmar que la copia termino sin errores y revisar el origen y los permisos. Si no coinciden, se debe volver a copiar desde una fuente confiable y no ejecutar ni distribuir el archivo.

## 5. Respaldos

Si los respaldos de lunes, martes y miercoles son incrementales, para recuperar el estado del miercoles se necesita el backup completo del domingo y todos los incrementales posteriores: lunes, martes y miercoles. Cada incremental contiene los cambios desde el respaldo anterior.

Si fueran diferenciales, se necesitaria el completo del domingo y solamente el diferencial del miercoles, porque ese archivo contiene los cambios acumulados desde el domingo. La eleccion intercambia tiempo de respaldo por simplicidad de recuperacion.

## 6. Caso integrador

Propongo una estrategia 3-2-1:

1. Tipo: backup completo semanal y backups incrementales diarios. Para datos especialmente criticos se puede agregar un registro de cambios frecuente.
2. Frecuencia: completo el domingo y incremental cada noche, despues de cerrar operaciones. Retener cuatro completos semanales y los incrementales necesarios.
3. Ubicacion: una copia en el servidor, otra en un medio diferente y una copia externa o en la nube con versionado. La copia externa debe ser inmutable o desconectada para reducir el impacto de ransomware.
4. Recuperacion: aislar el equipo afectado, verificar el ultimo backup valido, restaurar el completo mas reciente y aplicar los incrementales en orden. Para una eliminacion accidental se restaura el archivo o la version anterior sin reemplazar datos sanos.
5. Integridad: calcular SHA-256 antes del respaldo y despues de restaurar, comparar tamanos y metadatos importantes, abrir una muestra de archivos y revisar los registros de la tarea.

Esta estrategia limita la perdida maxima esperada al intervalo entre respaldos, ofrece varias copias ante fallas distintas y permite practicar la recuperacion. Las pruebas periodicas son necesarias: un backup que nunca se restaura no es una garantia.

## Procedimiento realizado

El archivo `practica.ps1` crea un archivo de datos, calcula su SHA-256, genera un respaldo completo y un respaldo incremental simulado, recupera el archivo y verifica que la huella recuperada coincida. Tambien crea `evidencia/prueba_integridad.txt` para demostrar que una copia alterada produce una huella diferente.

La ejecucion se registro en `evidencia/resultado_practica.txt`; los archivos generados quedan en `respaldo/` y `recuperado/`.

## Conclusion

El acceso indexado reduce el costo de busqueda, el indice invertido hace eficientes las consultas de texto, los formatos deben elegirse segun el intercambio requerido y SHA-256 permite detectar cambios. Una politica 3-2-1 con copias verificadas y restauraciones de prueba protege mejor la disponibilidad e integridad de la informacion.