# Changelog

## [1.3.3] - 2026-11-08 (F6/F9 staging-audit candidate)

- Sincroniza Dhole Realtime `rt-20261107-01` a versión global `1.13.12` tras incorporar auditoría real de persistencia en staging. PATCH 1.3.2 → 1.3.3; no modifica rutas, eventos, datos, contratos REST ni flags. No se fusiona hasta completar gates formales.


## [1.3.2] - 2026-11-08 (candidato F9, sin publicar)

- Sincroniza release global `rt-20261107-01` a `1.13.11` tras resolver los hallazgos de CI de Config, Reports y Gateway. Bump PATCH de DholeStorageService `1.3.1` a `1.3.2`, sin modificar REST, eventos, precios ni datos; flags realtime OFF y PR F9 aún DRAFT.


## [1.3.1] - 2026-11-08 (candidato F9, sin publicación)

- Sincroniza metadatos de versión global del batch Dhole Realtime `rt-20261107-01` a `1.13.10`; actualización PATCH independiente de DholeStorageService `1.3.0` → `1.3.1`. No cambia contratos REST, datos, lógica de negocio ni flags; requiere gates F0–F8 y CI de SHA final antes de merge.


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
