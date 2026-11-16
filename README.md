# TPI Grupo 6

Sistema de gestión para comercios con varias empresas y sucursales: stock por sucursal, ventas, compras de mercadería, actualización masiva de precios y reportes (ventas del día, ranking de productos y cierre mensual ajustado por inflación con datos del INDEC).

Hay dos roles:

- **Owner (dueño):** administra sus empresas, sucursales, productos, empleados, compras, precios y reportes.
- **Employee (empleado):** registra ventas y consulta el stock de la sucursal a la que está asignado.

## Tecnologías

- **Backend:** .NET 10, ASP.NET Core Web API, Entity Framework Core con SQLite, autenticación JWT y Swagger.
- **Frontend:** React 19, Vite y React Router.
- **Tests:** xUnit sobre la capa de aplicación, con repositorios en memoria.
- **CI/CD:** GitHub Actions. La API se publica como imagen Docker en GitHub Container Registry y el frontend en Vercel.

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
                   de configuración (JWT, CORS, Swagger, base de datos, proxy,
                   health check e inyección de dependencias)
tests/
  Application.Tests/  Tests de los servicios de Application
frontend/          Aplicación React + Vite (vercel.json para el despliegue en Vercel)
scripts/           Scripts del pipeline: disparo del despliegue y smoke tests
Dockerfile         Imagen de la API
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
- Swagger: `http://localhost:5015/swagger`. Para usar los endpoints protegidos, autenticate con `POST /api/authentication/authenticate`, copiá el token y pegalo en el botón **Authorize**.
- En desarrollo la configuración sale de `src/web/appsettings.Development.json`. La base SQLite (`miWebAppDatabase.db`) se crea sola y las migraciones se aplican al arrancar.

- Health check: `http://localhost:5015/health`.

Tests:

```bash
dotnet test MiWebApp.slnx
```

Con Docker (en `http://localhost:8080`):

```bash
docker build -t tpi-api .
docker run --rm -p 8080:8080   -e AuthenticationService__SecretForKey=<clave-de-al-menos-32-caracteres>   -e AuthenticationService__Issuer=tpigrupo6   -e AuthenticationService__Audience=tpigrupo6api   tpi-api
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
| `ConnectionStrings__DefaultConnection` | Sí | Cadena de conexión SQLite. En la imagen Docker vale `Data Source=/app/data/miWebAppDatabase.db`; la carpeta se crea al arrancar |
| `AuthenticationService__SecretForKey` | Sí | Clave para firmar los JWT (mínimo 32 caracteres) |
| `AuthenticationService__Issuer` | Sí | Emisor del token |
| `AuthenticationService__Audience` | Sí | Audiencia del token |
| `Cors__AllowedOrigins` | No | Orígenes permitidos, separados por comas. Por defecto `http://localhost:5173` |
| `Swagger__Enabled` | No | `true` para habilitar Swagger fuera de Development |
| `Seed__Enabled` | No | `true` para cargar los datos de prueba al iniciar si la base está vacía. En Development viene activado |
| `ExternalApis__SeriesBaseUrl` | No | URL de la API de series de datos.gob.ar. Por defecto `https://apis.datos.gob.ar/series/api/` |
| `ASPNETCORE_ENVIRONMENT` | No | `Development` o `Production`. En la imagen Docker vale `Production` |
| `PORT` | No | Puerto HTTP; la API escucha en `0.0.0.0:<PORT>`. En la imagen Docker vale `8080` |
| `HTTPS_PORT` | No | Puerto HTTPS público para la redirección (`443` detrás del proxy del hosting). Sin definir, no se redirige |
| `TZ` | No | Zona horaria del contenedor. En la imagen Docker vale `America/Argentina/Buenos_Aires` |
| `App__Version` | No | Versión que informa `/health`. El pipeline la completa con el commit desplegado |

### Frontend

En local se definen en `frontend/.env.development` (ver `frontend/.env.example`) y en producción en el proyecto de Vercel:

| Variable | Descripción |
|---|---|
| `VITE_API_URL` | URL base de la API. En local, por defecto `http://localhost:5015`. Es obligatoria para el build de producción |

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

### Pipeline

El workflow `.github/workflows/deploy.yml` corre en cada push y pull request a `main`:

1. **API:** restore, build, tests y build de la imagen Docker.
2. **Frontend:** `npm ci`, lint y build.

En cada push a `main`, si los dos jobs anteriores pasan:

3. **Deploy de la API:** publica la imagen en `ghcr.io/<owner>/<repo>/api` (tags `latest` y `sha-<commit>`), dispara el deploy hook del hosting si está configurado y corre `scripts/smoke-test-api.sh`: espera a que `/health` informe la versión del commit, verifica que sin token responda 401, inicia sesión con un usuario del seed, consulta `/api/me` y comprueba CORS con el dominio del frontend.
4. **Deploy del frontend:** build y despliegue en Vercel con la CLI y `scripts/smoke-test-frontend.sh`, que verifica la página principal y una ruta interna.

No hay credenciales en el repositorio: todo sale de los secrets y variables de GitHub.

| Tipo | Nombre | Valor |
|---|---|---|
| Secret | `DEPLOY_HOOK_URL` | Deploy hook del hosting de la API (opcional) |
| Secret | `VERCEL_TOKEN` | Token de la cuenta de Vercel |
| Secret | `VERCEL_ORG_ID` | ID de la cuenta o equipo de Vercel |
| Secret | `VERCEL_PROJECT_ID` | ID del proyecto de Vercel |
| Secret | `SMOKE_PASSWORD` | Contraseña del usuario del smoke test (`dueno123`) |
| Variable | `API_URL` | URL pública de la API |
| Variable | `FRONTEND_URL` | URL pública del frontend |
| Variable | `SMOKE_CUIT` | CUIT del usuario del smoke test (`20-30111222-3`) |

### API

La API corre en cualquier hosting que ejecute contenedores Docker, usando la imagen de GitHub Container Registry (hay que hacer público el paquete) o construyéndola desde el `Dockerfile` del repositorio.

- Health check: `/health`.
- Variables obligatorias en el hosting: `AuthenticationService__SecretForKey`, `AuthenticationService__Issuer`, `AuthenticationService__Audience` y `Cors__AllowedOrigins` con el dominio de Vercel. Se recomienda `HTTPS_PORT=443`.
- El secreto JWT solo se define como variable de entorno del servidor; no está en `appsettings.json` ni en el pipeline.

**Base de datos:** se usa SQLite dentro del contenedor. En los planes gratuitos el disco es efímero, así que la base se pierde en cada redeploy o reinicio. Al arrancar, la API crea la carpeta de la base, aplica las migraciones y, como la imagen trae `Seed__Enabled=true`, vuelve a cargar los datos de prueba. Los datos cargados a mano no se conservan entre reinicios.

### Frontend (Vercel)

1. Importar el repositorio en Vercel con **Root Directory** `frontend`.
2. Definir `VITE_API_URL` con la URL pública de la API en el entorno Production.
3. `frontend/vercel.json` redirige todas las rutas a `index.html` para que funcione React Router y desactiva el despliegue automático de Vercel: el frontend se despliega solo desde el pipeline.
