# Correcciones de compilación

- Reemplazado el resolver antiguo que dependía de `CustomCodeFramework.Storage.Abstractions`.
- El resolver de compatibilidad delega en `IStorageObjectStoreResolver`.
- Las librerías compartidas no generan `.deps.json`.
- Incluido `run-dev.sh` para compilar de forma serial y ejecutar API/Workers con `--no-build`.

Ejecutar:

```bash
chmod +x run-dev.sh
./run-dev.sh
```
