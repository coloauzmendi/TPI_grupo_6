FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/web/web.csproj src/web/
RUN dotnet restore src/web/web.csproj

COPY src/ src/
RUN dotnet publish src/web/web.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ARG APP_VERSION=""

ENV ASPNETCORE_ENVIRONMENT=Production \
    PORT=8080 \
    TZ=America/Argentina/Buenos_Aires \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/miWebAppDatabase.db" \
    Seed__Enabled=true \
    App__Version=$APP_VERSION

RUN mkdir -p /app/data && chown -R $APP_UID /app/data

COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "web.dll"]
