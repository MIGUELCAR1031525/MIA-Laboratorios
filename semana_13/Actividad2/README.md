# MIA Azure Blob

## 4. Seguridad

La Connection String contiene credenciales sensibles. No debe publicarse en GitHub ni compartirse en repositorios, capturas o mensajes.

### Recomendación

- No codifiques la Connection String directamente en el código fuente.
- Usa una variable de entorno o un archivo local que esté ignorado por Git.
- Mantén el archivo `appsettings.Development.json` solo en tu máquina local.
- Añade siempre un `.gitignore` para evitar subir secretos.

### Archivo local `.env`

En la carpeta principal del proyecto, copia `.env.example` como `.env` y reemplaza el marcador por la Connection String de Azure:

```env
AZURE_STORAGE_CONNECTION_STRING=TU_CONNECTION_STRING
```

La aplicación carga `.env` al iniciar. También acepta una variable de entorno de Windows o, como alternativa local, `AzureStorage:ConnectionString` en `appsettings.Development.json`.
Entrega `.env.example` como plantilla, pero nunca subas el archivo `.env` real.

### Importante

- Nunca subas la Connection String a GitHub.
- El archivo `.env` está incluido en `.gitignore`.
- Si usas `appsettings.Development.json`, asegúrate de que esté listado en `.gitignore`.
- En un entorno real, se recomienda usar Azure Key Vault o variables del sistema para gestionar secretos con mayor seguridad.

La aplicación carga `.env` y lee `AZURE_STORAGE_CONNECTION_STRING`; si no está definida, busca `AzureStorage:ConnectionString` en el archivo local.
