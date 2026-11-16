// Infraestructura de la Web API en Azure App Service (nivel gratuito).
// Se despliega a nivel de grupo de recursos:
//   az deployment group create -g <grupo> -f infra/main.bicep -p jwtSecret=<secreto> ...
// Ningun valor sensible se escribe en este archivo: el secreto JWT llega como parametro seguro.

targetScope = 'resourceGroup'

@description('Region de Azure donde se crean los recursos.')
param location string = resourceGroup().location

@description('Nombre de la Web App. Debe ser unico en todo Azure: queda como <nombre>.azurewebsites.net.')
@minLength(2)
@maxLength(60)
param webAppName string = 'tpigrupo6-api-${uniqueString(resourceGroup().id)}'

@description('Nombre del App Service Plan.')
param appServicePlanName string = '${webAppName}-plan'

@description('Clave para firmar los JWT (minimo 32 caracteres). Se pasa desde un secreto del pipeline.')
@secure()
@minLength(32)
param jwtSecret string

@description('Issuer de los JWT. Por defecto, la URL publica de la Web App.')
param jwtIssuer string = 'https://${webAppName}.azurewebsites.net'

@description('Audience de los JWT.')
param jwtAudience string = 'tpigrupo6api'

@description('Origenes permitidos por CORS, separados por coma (por ejemplo la URL del frontend).')
param corsAllowedOrigins string = 'http://localhost:5173'

// Ruta del archivo SQLite: /home es el unico almacenamiento persistente en App Service Linux
var sqliteConnectionString = 'Data Source=/home/data/miWebAppDatabase.db'

// Plan Linux gratuito. El SKU queda fijo para no generar costos por error.
resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  properties: {
    reserved: true // obligatorio para planes Linux
  }
}

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      // Proyecto publicado: el ensamblado de entrada es web.dll
      appCommandLine: 'dotnet web.dll'
      alwaysOn: false // el plan F1 no permite Always On
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          // Mantiene /home montado y persistente entre reinicios (alli vive la base SQLite)
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'true'
        }
        {
          // El pipeline sube la aplicacion ya compilada: no hace falta compilar en el servidor
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        {
          name: 'WEBSITE_TIME_ZONE'
          value: 'America/Argentina/Buenos_Aires'
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: sqliteConnectionString
        }
        {
          name: 'AuthenticationService__SecretForKey'
          value: jwtSecret
        }
        {
          name: 'AuthenticationService__Issuer'
          value: jwtIssuer
        }
        {
          name: 'AuthenticationService__Audience'
          value: jwtAudience
        }
        {
          name: 'Cors__AllowedOrigins'
          value: corsAllowedOrigins
        }
      ]
    }
  }
}

// El despliegue se hace con OIDC (Microsoft Entra ID), asi que se deshabilitan
// las credenciales basicas de publicacion por FTP y por SCM.
resource ftpBasicAuth 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-04-01' = {
  parent: webApp
  name: 'ftp'
  properties: {
    allow: false
  }
}

resource scmBasicAuth 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-04-01' = {
  parent: webApp
  name: 'scm'
  properties: {
    allow: false
  }
}

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
