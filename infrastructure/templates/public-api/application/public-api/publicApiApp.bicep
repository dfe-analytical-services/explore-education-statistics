@description('Specifies the location for all resources.')
param location string

@description('Specifies the Public API resource prefix')
param publicApiResourcePrefix string

@description('Specifies the Admin App Service name.')
param adminAppName string

@description('Specifies the id of the Container App Environment in which to deploy this Container App.')
param containerAppEnvironmentId string

@description('Specifies the name of the fileshare used to store Public API Data.')
param publicApiDataFileShareName string

@description('Specifies the Container Registry name from which to pull Docker images.')
param acrName string

@description('The tags of the Docker images to deploy.')
param dockerImagesTag string

@description('The URL of the Public API.')
param publicApiUrl string

@description('The URL of the Cotnent API.')
param contentApiUrl string

@description('Specifies the Application (Client) Id of the App Registration used to represent the API Container App.')
param apiAppRegistrationClientId string

@description('Specifies the connection string template for the Public API PSQL database.')
param publicApiDbConnectionStringTemplate string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var apiAppName = '${publicApiResourcePrefix}-ca-api'
var apiAppIdentityName = '${publicApiResourcePrefix}-id-ca-api'

var dataFilesFileShareMountPath = '/data/public-api-data'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
}

resource adminAppService 'Microsoft.Web/sites@2023-12-01' existing = {
  name: adminAppName
}

resource adminAppServiceIdentity 'Microsoft.ManagedIdentity/identities@2023-01-31' existing = {
  scope: adminAppService
  name: 'default'
}

var adminAppClientId = adminAppServiceIdentity.properties.clientId
var adminAppPrincipalId = adminAppServiceIdentity.properties.principalId

resource apiContainerAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: apiAppIdentityName
  location: location
}

module apiContainerAppAcrPullRoleAssignmentModule '../../components/containerRegistryRoleAssignment.bicep' = {
  name: '${apiAppIdentityName}AcrPullRoleAssignmentDeploy'
  params: {
    role: 'AcrPull'
    containerRegistryName: acrName
    principalIds: [apiContainerAppManagedIdentity.properties.principalId]
  }
}

module apiContainerAppModule '../../components/containerApp.bicep' = {
  name: 'apiContainerAppDeploy'
  params: {
    location: location
    containerAppName: apiAppName
    acrLoginServer: containerRegistry.properties.loginServer
    containerAppImageName: 'ees-public-api/api:${dockerImagesTag}'
    userAssignedManagedIdentityId: apiContainerAppManagedIdentity.id
    managedEnvironmentId: containerAppEnvironmentId
    corsPolicy: {
      allowedOrigins: [
        publicApiUrl
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
        storageName: publicApiDataFileShareName
      }
    ]
    appSettings: [
      {
        name: 'ConnectionStrings__PublicDataDb'
        value: replace(replace(publicApiDbConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', apiAppIdentityName)
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
        name: 'ContentApi__Url'
        value: contentApiUrl
      }
      {
        name: 'MiniProfiler__Enabled'
        value: 'true'
      }
      {
        name: 'DataFiles__BasePath'
        value: dataFilesFileShareMountPath
      }
      {
        name: 'OpenIdConnect__TenantId'
        value: tenant().tenantId
      }
      {
        name: 'OpenIdConnect__ClientId'
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
  dependsOn: [
    apiContainerAppAcrPullRoleAssignmentModule
  ]
}

output containerAppFqdn string = apiContainerAppModule.outputs.containerAppFqdn
output containerAppName string = apiContainerAppModule.outputs.containerAppName
output containerAppHealthProbeRelativeUrl string = '/docs'
