import { resourceNamesType } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames resourceNamesType

@description('Specifies the location for all resources.')
param location string

@description('Specifies the id of the Container App Environment in which to deploy this Container App.')
param containerAppEnvironmentId string

@description('The tags of the Docker images to deploy.')
param dockerImagesTag string

@description('The URL of the Public API.')
param publicApiUrl string

@description('The URL of the Public site.')
param publicSiteUrl string

@description('The URL of the Content API.')
param contentApiUrl string

@description('Specifies the Application (Client) Id of the App Registration used to represent the API Container App.')
param apiAppRegistrationClientId string

@description('Specifies the Application Insights connection string for this Container App to use for its monitoring.')
param appInsightsConnectionString string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var dataFilesFileShareMountPath = '/data/public-api-data'

resource adminAppService 'Microsoft.Web/sites@2023-12-01' existing = {
  name: resourceNames.existingResources.adminApp
}

resource adminAppServiceIdentity 'Microsoft.ManagedIdentity/identities@2023-01-31' existing = {
  scope: adminAppService
  name: 'default'
}

var adminAppClientId = adminAppServiceIdentity.properties.clientId
var adminAppPrincipalId = adminAppServiceIdentity.properties.principalId

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.existingResources.keyVault
}

resource apiContainerAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: resourceNames.publicApi.apiAppIdentity
}

module apiContainerAppModule '../../components/containerApp.bicep' = {
  name: 'apiContainerAppDeploy'
  params: {
    location: location
    containerAppName: resourceNames.publicApi.apiApp
    acrLoginServer: keyVault.getSecret('DOCKER-REGISTRY-SERVER-DOMAIN')
    containerAppImageName: 'ees-public-api/api:${dockerImagesTag}'
    dockerPullManagedIdentityClientId: keyVault.getSecret('DOCKER-REGISTRY-SERVER-USERNAME')
    dockerPullManagedIdentitySecretValue: keyVault.getSecret('DOCKER-REGISTRY-SERVER-PASSWORD')
    userAssignedManagedIdentityId: apiContainerAppManagedIdentity.id
    managedEnvironmentId: containerAppEnvironmentId
    corsPolicy: {
      allowedOrigins: [
        publicSiteUrl
        'http://localhost:3000'
        'http://127.0.0.1'
      ]
    }
    volumeMounts: [
      {
        volumeName: 'public-api-fileshare-mount'
        mountPath: dataFilesFileShareMountPath
      }
    ]
    volumes: [
      {
        name: 'public-api-fileshare-mount'
        storageType: 'AzureFile'
        storageName: resourceNames.publicApi.publicApiFileshare
      }
    ]
    appSettings: [
      {
        name: 'ConnectionStrings:PublicDataDb'
        value: 'Server=${resourceNames.sharedResources.postgreSqlFlexibleServer}.postgres.database.azure.com;Database=public_data;Port=5432;User Id=${resourceNames.publicApi.apiAppIdentity}'
      }
      {
        // This settings allows the Container App to identify which user-assigned identity it should use in order to
        // perform Managed Identity-based authentication and authorization with other Azure services / resources.
        //
        // It is used in conjunction with the Azure.Identity .NET library to retrieve access tokens for the user-assigned
        // identity.
        name: 'AZURE_CLIENT_ID'
        value: apiContainerAppManagedIdentity.properties.clientId
      }
      {
        name: 'App:Url'
        value: publicApiUrl
      }
      {
        name: 'AppInsights:ConnectionString'
        value: appInsightsConnectionString
      }
      {
        name: 'ContentApi:Url'
        value: contentApiUrl
      }
      {
        name: 'MiniProfiler:Enabled'
        value: 'true'
      }
      {
        name: 'DataFiles:BasePath'
        value: dataFilesFileShareMountPath
      }
      {
        name: 'OpenIdConnect:TenantId'
        value: tenant().tenantId
      }
      {
        name: 'OpenIdConnect:ClientId'
        value: apiAppRegistrationClientId
      }
    ]
    entraIdAuthentication: {
      appRegistrationClientId: apiAppRegistrationClientId
      allowedClientIds: [
        adminAppClientId
      ]
      allowedPrincipalIds: [
        adminAppPrincipalId
      ]
      requireAuthentication: false
    }
    tagValues: tagValues
  }
}

output containerAppFqdn string = apiContainerAppModule.outputs.containerAppFqdn
output containerAppName string = apiContainerAppModule.outputs.containerAppName
output containerAppHealthProbeRelativeUrl string = '/docs'
