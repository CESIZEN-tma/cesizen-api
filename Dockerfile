FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# APP_UID is a non-root UID defined by the base image itself (Microsoft's
# recommended non-root pattern for .NET 8+ containers), not left unset.
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_USERNAME

WORKDIR /src

RUN --mount=type=secret,id=github_token \
    dotnet nuget add source "https://nuget.pkg.github.com/TitouanML/index.json" --name GitHub --username $GITHUB_USERNAME --password "$(cat /run/secrets/github_token)" --store-password-in-clear-text

COPY ["api.csproj", "./"]
RUN dotnet restore "api.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "api.dll"]
