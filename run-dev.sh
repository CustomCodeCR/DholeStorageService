#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")"

# Evita que API y Workers intenten generar los mismos artefactos al mismo tiempo.
rm -f src/Dhole.Storage.Domain/Shared/StorageConstanst.cs
find src -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
dotnet restore "DholeStorageService.slnx"
dotnet build "DholeStorageService.slnx" --no-restore -m:1

dotnet run --project "src/Dhole.Storage.Api/Dhole.Storage.Api.csproj" --no-build > "/tmp/Dhole.Storage.Api.log" 2>&1 &
echo "Iniciado Dhole.Storage.Api. Log: /tmp/Dhole.Storage.Api.log"
dotnet run --project "src/Dhole.Storage.Workers/Dhole.Storage.Workers.csproj" --no-build > "/tmp/Dhole.Storage.Workers.log" 2>&1 &
echo "Iniciado Dhole.Storage.Workers. Log: /tmp/Dhole.Storage.Workers.log"

wait
