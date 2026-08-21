# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 5207

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG GITHUB_ACTOR=github-actions
WORKDIR /src

COPY ["NuGet.Config", "./NuGet.Config"]
COPY ["src/Dhole.Storage.Api/Dhole.Storage.Api.csproj", "src/Dhole.Storage.Api/"]
COPY ["src/Dhole.Storage.Application/Dhole.Storage.Application.csproj", "src/Dhole.Storage.Application/"]
COPY ["src/Dhole.Storage.Contracts/Dhole.Storage.Contracts.csproj", "src/Dhole.Storage.Contracts/"]
COPY ["src/Dhole.Storage.Domain/Dhole.Storage.Domain.csproj", "src/Dhole.Storage.Domain/"]
COPY ["src/Dhole.Storage.Infrastructure/Dhole.Storage.Infrastructure.csproj", "src/Dhole.Storage.Infrastructure/"]
COPY ["src/Dhole.Storage.Persistence/Dhole.Storage.Persistence.csproj", "src/Dhole.Storage.Persistence/"]

RUN --mount=type=secret,id=github_token,required=true \
  export NuGetPackageSourceCredentials_github="Username=${GITHUB_ACTOR};Password=$(cat /run/secrets/github_token);ValidAuthenticationTypes=Basic" && \
  dotnet restore "src/Dhole.Storage.Api/Dhole.Storage.Api.csproj" --configfile /src/NuGet.Config
COPY . .
RUN dotnet publish "src/Dhole.Storage.Api/Dhole.Storage.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_ENVIRONMENT=Production
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Dhole.Storage.Api.dll"]
