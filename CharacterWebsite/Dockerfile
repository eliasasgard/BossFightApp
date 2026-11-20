# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

COPY . .

RUN dotnet publish "CharacterWebsite/CharacterWebsite.csproj" \
    -c Release \
    -o /app/publish \
    --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

USER $APP_UID

ENTRYPOINT ["dotnet", "CharacterWebsite.dll"]