# Changelog

## [1.3.0] - 2026-11-08 (candidato F6.07 no liberado)

- F6.07: captura transaccional de cambios de archivos, versiones y referencias dentro del Outbox existente de Storage, sin revelar contenido, rutas internas, nombre de archivo, checksum ni credenciales.
- El worker publica en `dhole.storage.realtime.v1` independientemente de eventos comerciales; REST autorizado sigue siendo la fuente de los datos.
- Flags OFF por defecto, pruebas de contrato y versionado candidato del componente 1.2.2 → 1.3.0, global 1.13.0. Sin merges a targets ni deploy.

## [1.2.2] - 2026-11-05

- Tolera fallos transitorios de DNS durante la clonación de DholeSystem mediante reintentos acotados en staging y producción.

## [1.2.1] - 2026-11-05

- Consolida SemVer por componente y despliegues automáticos a staging/producción por push.
- Sincroniza versión nativa y manifiesto del release global Dhole.

## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
