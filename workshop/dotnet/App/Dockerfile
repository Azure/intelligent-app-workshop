FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["../Directory.Build.props", "."]
COPY ["../Directory.Packages.props", "."]
COPY ["App/Directory.Build.props", "App/"]
COPY ["App/backend/", "App/backend/"]
COPY ["../../Core.Utilities/", "Core.Utilities/"]
RUN dotnet restore "App/backend/backend.csproj"

WORKDIR "/src/App/backend"
FROM build AS publish
RUN dotnet publish "backend.csproj" -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "backend.dll"]