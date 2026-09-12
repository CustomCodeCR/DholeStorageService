# FASE 7 — Storage para Mercadeo

Esta fase implementa el procesamiento especializado de multimedia dentro de `DholeStorageService`.

## Responsabilidad

Storage conserva los archivos físicos y sus derivados. `DholeContentService` continúa guardando únicamente referencias (`MediaReference` / `content_media`).

## Endpoint

`POST /api/v1/storage/marketing/files`

Usa `multipart/form-data` con los mismos campos de referencia del upload general:

- `file`
- `sourceService`
- `entityType`
- `entityId`
- `providerId` opcional
- `metadataJson` opcional

## Formatos iniciales

Imágenes:

- JPG/JPEG
- PNG
- WebP
- AVIF

Videos:

- MP4
- WebM
- MOV

Documentos:

- PDF
- DOC/DOCX
- XLS/XLSX
- PPT/PPTX
- CSV
- TXT

## Validaciones

La carga especializada valida:

1. tamaño por categoría;
2. extensión permitida;
3. compatibilidad extensión ↔ MIME type;
4. firma básica del contenido para evitar archivos renombrados o MIME falsificado.

Valores por defecto:

- imágenes: 25 MB;
- videos: 100 MB;
- documentos/PDF: 50 MB.

Se pueden sobrescribir mediante `MarketingMedia:*`.

## Imágenes

Por cada imagen se conserva el original y se generan:

- `thumbnail.webp`, máximo 480×480;
- `optimized.webp`, máximo 1920×1920.

También se registra formato, ancho y alto.

## Videos

El runtime incluye `ffprobe` y `ffmpeg`.

Para videos se obtiene:

- duración;
- ancho;
- alto;
- formato/codec;
- `poster.jpg` cuando el video permite extraer un frame.

El video nunca se guarda como bytes en PostgreSQL.

## Metadata

Los derivados se guardan en el mismo proveedor configurado para el archivo original. Sus rutas y propiedades se agregan dentro de `File.MetadataJson` bajo `marketingMedia`.

PostgreSQL conserva únicamente metadata y rutas; los bytes permanecen en Local/S3/MinIO/Azure según el provider.

## Compatibilidad

El endpoint genérico `/api/v1/storage/files` se mantiene sin cambios para no romper cargas de otros microservicios. Mercadeo debe usar el endpoint especializado para obtener validación y procesamiento de FASE 7.
