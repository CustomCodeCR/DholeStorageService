# Dhole Storage Service

## Puerto y responsabilidades

- API HTTP: `5207`
- Persiste archivos importados, correos completos (`.eml`) y adjuntos.
- Devuelve referencias opacas `storage://{fileId}`; los servicios consumidores no conocen la ruta física.
- Proveedores soportados: Local, MinIO, Amazon S3 y Azure Blob Storage.
- Publica eventos de negocio y auditoría mediante Outbox desde `Dhole.Storage.Workers`.

## Inicio local

La base PostgreSQL `dhole_storage` debe existir. En el primer inicio se crean las tablas y el proveedor Local predeterminado.

```bash
dotnet run --project src/Dhole.Storage.Api
dotnet run --project src/Dhole.Storage.Workers
```

Después se inician DataExtraction API y Workers. Ambos tienen por defecto:

```json
"StorageService": {
  "Enabled": true,
  "Address": "http://localhost:5207",
  "TimeoutSeconds": 120
}
```

En Docker, use el nombre DNS del contenedor en `StorageService__Address`, por ejemplo `http://dhole-storage-api:5207`.

## Flujo de DataExtraction

- Correo: guarda el mensaje MIME completo como `EmailMessage/raw.eml`.
- Adjunto: guarda cada binario como `EmailAttachment` y conserva `EmailMessageId` en metadata.
- Importación manual/API: guarda el archivo como `ExtractionExecutionSource` antes de extraerlo.
- Los registros antiguos con rutas locales siguen siendo legibles como compatibilidad.

## Proveedor local

La ruta predeterminada es:

```text
./storage/dhole-storage
```

Puede cambiarse con:

```bash
export Storage__Local__RootPath=/ruta/absoluta/dhole-storage
```

## MinIO/S3

Cree un proveedor con configuración JSON similar a:

```json
{
  "bucketName": "dhole-files",
  "serviceUrl": "http://localhost:9000",
  "region": "us-east-1",
  "accessKey": "minioadmin",
  "secretKey": "minioadmin",
  "forcePathStyle": true,
  "useHttp": true
}
```

Para S3 real puede omitir `serviceUrl` y usar credenciales del entorno o del rol de instancia.

## Azure Blob

```json
{
  "connectionString": "<connection-string>",
  "containerName": "dhole-files"
}
```
