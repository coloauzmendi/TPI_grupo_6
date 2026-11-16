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
| `Seed__Enabled` | No | `true` para cargar los datos de prueba al iniciar si la base está vacía. En Development viene activado |
| `ExternalApis__SeriesBaseUrl` | No | URL de la API de series de datos.gob.ar. Por defecto `https://apis.datos.gob.ar/series/api/` |
| `ASPNETCORE_ENVIRONMENT` | No | `Development` o `Production` |

### Frontend

Se definen en `frontend/.env.development` (ver `frontend/.env.example`):

| Variable | Descripción |
|---|---|
| `VITE_API_URL` | URL base de la API. Por defecto `http://localhost:5015` |

## Datos de prueba

Al iniciar la API, después de aplicar las migraciones, se cargan datos de prueba si `Seed:Enabled` está en `true` y la base está vacía. Nunca duplica ni modifica datos existentes. En `appsettings.Development.json` viene activado y en `appsettings.json` desactivado; se cambia con la variable de entorno `Seed__Enabled=true` o `false`.

El inicio de sesión es con CUIT y contraseña:

| Rol | Nombre | CUIT | Contraseña | Acceso |
|---|---|---|---|---|
| Owner | Martín Gómez | `20-30111222-3` | `dueno123` | Kiosco El Sol y Almacén Don José |
| Owner | Laura Fernández | `27-28444555-6` | `dueno123` | Ferretería La Tuerca |
| Employee | Sofía Ramírez | `27-35111222-4` | `empleado123` | Kiosco El Sol, Córdoba 1234 (Rosario) |
| Employee | Lucas Díaz | `20-36222333-5` | `empleado123` | Kiosco El Sol, Alberdi 850 (Rosario) |
| Employee | Valentina López | `27-37333444-6` | `empleado123` | Almacén Don José, Hipólito Yrigoyen 2100 (Funes) |
| Employee | Joaquín Torres | `20-38444555-7` | `empleado123` | Ferretería La Tuerca, San Martín 3200 (Rosario) |
| Employee | Camila Sosa | `27-39555666-8` | `empleado123` | Ferretería La Tuerca, Av. San Martín 900 (Pérez) |

Qué carga:

- 3 empresas con 7 sucursales en total y los usuarios de la tabla.
- Productos por empresa, incluidos uno sin stock en ninguna sucursal y dos dados de baja.
- Stock por sucursal con mínimos: algunos productos quedan por debajo del mínimo y uno en 0.
- Compras con costo unitario: la carga inicial de cada producto y reposiciones posteriores.
- Ventas de los últimos 30 días, de dueños y de empleados, con el precio unitario guardado y dos anuladas.

La inflación no se carga: se obtiene del servicio externo.

## Despliegue

El workflow `.github/workflows/deploy.yml` compila y corre los tests en cada pull request a `main`. En cada push a `main` además despliega la infraestructura definida en `infra/main.bicep` y publica la API en Azure App Service. La autenticación con Azure se hace por OIDC, y el secreto JWT se pasa como secret del repositorio (`JWT_SECRET`).
