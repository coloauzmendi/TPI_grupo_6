# TPI Grupo 6

Sistema de gestión para comercios con varias empresas y sucursales: stock por sucursal, ventas, compras de mercadería, actualización masiva de precios y reportes (ventas del día, ranking de productos y cierre mensual ajustado por inflación con datos del INDEC).

Hay dos roles:

- **Owner (dueño):** administra sus empresas, sucursales, productos, empleados, compras, precios y reportes.
- **Employee (empleado):** registra ventas y consulta el stock de la sucursal a la que está asignado.

## Tecnologías

- **Backend:** .NET 10, ASP.NET Core Web API, Entity Framework Core con SQLite, autenticación JWT y Swagger.
- **Frontend:** React 19, Vite y React Router.
- **Tests:** xUnit sobre la capa de aplicación, con repositorios en memoria.
- **CI/CD:** GitHub Actions y Bicep, con despliegue en Azure App Service.

## Estructura

```
src/
  Domain/          Entidades, enums, excepciones e interfaz ISoftDelete (sin dependencias)
  Application/     Casos de uso: servicios, modelos de request/response e interfaces
                   de repositorios y servicios (depende de Domain)
  Infrastructure/  Implementaciones: ApplicationContext y migraciones de EF Core,
                   repositorios, JWT, hash de contraseñas y cliente de la API de
                   inflación (depende de Application)
  web/             API: controllers, manejo global de errores y extension methods
                   de configuración (JWT, CORS, Swagger, base de datos e inyección
                   de dependencias)
tests/
  Application.Tests/  Tests de los servicios de Application
frontend/          Aplicación React + Vite
infra/main.bicep   Infraestructura de Azure (App Service Linux)
.github/workflows/ Pipeline de build, test y deploy
```

Las dependencias apuntan hacia adentro: `web` → `Infrastructure` → `Application` → `Domain`. Los controllers y servicios dependen de interfaces, y las implementaciones se registran en `web/Extensions/ServiceCollectionExtensions.cs`.

Los repositorios heredan de `Repository<T>`, que resuelve el alta, la modificación y la baja. Las entidades que implementan `ISoftDelete` (empresas, sucursales, usuarios, productos y stock) se dan de baja de forma lógica (`IsActive = false`). Las demás se eliminan.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20.19 o superior (o 22.12 o superior)
- Opcional: la herramienta `dotnet-ef` para crear migraciones (`dotnet tool install --global dotnet-ef`)

## Cómo correr el backend

```bash
dotnet run --project src/web --launch-profile http
```

- La API queda en `http://localhost:5015` (con el perfil `https`, también en `https://localhost:7224`).
- Swagger: `http://localhost:5015/swagger`. Para usar los endpoints protegidos, autenticate con `POST /api/Authentication`, copiá el token y pegalo en el botón **Authorize**.
- En desarrollo la configuración sale de `src/web/appsettings.Development.json`. La base SQLite (`miWebAppDatabase.db`) se crea sola y las migraciones se aplican al arrancar.

Tests:

```bash
dotnet test MiWebApp.slnx
```

Nueva migración:

```bash
dotnet ef migrations add <Nombre> --project src/Infrastructure --startup-project src/web
```

## Cómo correr el frontend

```bash
cd frontend
npm install
npm run dev
```

El frontend queda en `http://localhost:5173`, que es el origen que el backend permite por defecto en CORS.

## Variables de entorno

### Backend

En producción se configuran como variables de entorno; el `:` de la clave se reemplaza por `__`. Si falta un valor obligatorio, la API no arranca e indica cuál falta.

| Variable | Obligatoria | Descripción |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Sí | Cadena de conexión SQLite, por ejemplo `Data Source=/home/data/app.db` |
| `AuthenticationService__SecretForKey` | Sí | Clave para firmar los JWT (mínimo 32 caracteres) |
| `AuthenticationService__Issuer` | Sí | Emisor del token |
| `AuthenticationService__Audience` | Sí | Audiencia del token |
| `Cors__AllowedOrigins` | No | Orígenes permitidos, separados por comas. Por defecto `http://localhost:5173` |
| `Swagger__Enabled` | No | `true` para habilitar Swagger fuera de Development |
| `ExternalApis__SeriesBaseUrl` | No | URL de la API de series de datos.gob.ar. Por defecto `https://apis.datos.gob.ar/series/api/` |
| `ASPNETCORE_ENVIRONMENT` | No | `Development` o `Production` |

### Frontend

Se definen en `frontend/.env.development` (ver `frontend/.env.example`):

| Variable | Descripción |
|---|---|
| `VITE_API_URL` | URL base de la API. Por defecto `http://localhost:5015` |

## Despliegue

El workflow `.github/workflows/deploy.yml` compila y corre los tests en cada pull request a `main`. En cada push a `main` además despliega la infraestructura definida en `infra/main.bicep` y publica la API en Azure App Service. La autenticación con Azure se hace por OIDC, y el secreto JWT se pasa como secret del repositorio (`JWT_SECRET`).
