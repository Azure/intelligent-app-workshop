FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["../Directory.Build.props", "."]
COPY ["../Directory.Packages.props", "."]
COPY ["App/Directory.Build.props", "App/"]
COPY ["App/backend/", "App/backend/"]
COPY ["../../Core.Utilities/", "Core.Utilities/"]
RUN dotnet restore "App/backend/MinimalApi.csproj"

WORKDIR "/src/App/backend"
FROM build AS publish
RUN dotnet publish "MinimalApi.csproj" -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MinimalApi.dll"]