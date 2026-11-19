# Changelog

## [1.2.3] - 2026-11-18

- PATCH DH-20261118-SRM-ATTACHMENT-METADATA-240: `FileDto` expone `metadataJson` para visualizar la descripción y vigencia de adjuntos SRM sin perder la propiedad histórica `metadatJson`.
- Almacenamiento de archivos y metadatos sin migración, compatible con adjuntos previos; acompañamiento de DholeWeb 1.83.2, ecosistema objetivo 1.87.5.

## [1.2.2] - 2026-11-05

- Retries DholeSystem clone on transient GitHub DNS failures in staging and production.


## [1.2.1] - 2026-11-05

- Actualiza versión SemVer del componente y metadatos del ecosistema Dhole 1.3.1.
- Garantiza despliegues por push en master/develop y preserva las comprobaciones CI.

## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
