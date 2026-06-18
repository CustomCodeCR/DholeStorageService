# Base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && \
  apt-get install -y tzdata apt-utils libgdiplus libc6-dev && \
  ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll && \
  rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 8080
ENV TZ=America/Costa_Rica
ENV ASPNETCORE_URLS=http://+:8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Dhole.Storage.Api/Dhole.Storage.Api.csproj", "src/Dhole.Storage.Api/"]
COPY ["src/Dhole.Storage.Application/Dhole.Storage.Application.csproj", "src/Dhole.Storage.Application/"]
COPY ["src/Dhole.Storage.Domain/Dhole.Storage.Domain.csproj", "src/Dhole.Storage.Domain/"]
COPY ["src/Dhole.Storage.Persistence/Dhole.Storage.Persistence.csproj", "src/Dhole.Storage.Persistence/"]
COPY ["src/Dhole.Storage.Infrastructure/Dhole.Storage.Infrastructure.csproj", "src/Dhole.Storage.Infrastructure/"]
COPY ["src/Dhole.Storage.Contracts/Dhole.Storage.Contracts.csproj", "src/Dhole.Storage.Contracts/"]

RUN dotnet restore "src/Dhole.Storage.Api/Dhole.Storage.Api.csproj"

COPY . .

WORKDIR "/src/src/Dhole.Storage.Api"
RUN dotnet build "Dhole.Storage.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publish image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/src/Dhole.Storage.Api

RUN dotnet publish "Dhole.Storage.Api.csproj" \
  -c $BUILD_CONFIGURATION \
  -o /app/publish \
  /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod 755 /app

ENTRYPOINT ["dotnet", "Dhole.Storage.Api.dll"]
