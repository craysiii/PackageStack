FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["./PackageStack.Api/PackageStack.Api.csproj", "PackageStack.Api/"]
COPY ["./PackageStack.Common/PackageStack.Common.csproj", "PackageStack.Common/"]

RUN dotnet restore "./PackageStack.Api/PackageStack.Api.csproj"
RUN dotnet restore "./PackageStack.Common/PackageStack.Common.csproj"
COPY . .

WORKDIR "/src/PackageStack.Api"
RUN dotnet build "./PackageStack.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PackageStack.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PackageStack.Api.dll"]
