import { ResourceNames, ContainerAppResourceConfig, SearchServiceConfig } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('The ID of the owning Container App Environment.')
param containerAppEnvironmentId string

@description('The IP address of the owning Container App Environment.')
param containerAppEnvironmentIpAddress string

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

@description('Resource limits and scaling configuration.')
param resourceAndScalingConfig ContainerAppResourceConfig

@description('Configuration settings for the Search service used to search publications.')
param searchServiceConfig SearchServiceConfig

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Enable the Swagger UI')
param enableSwagger bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var analyticsFileShareMountPath = '/data/analytics'

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
    environmentId: containerAppEnvironmentId
    environmentIpAddress: containerAppEnvironmentIpAddress
    deployPrivateDns: true
    corsPolicy: {
      allowedOrigins: [
        publicSiteUrl
        'http://localhost:3000'
        'http://127.0.0.1'
      ]
    }
    vnetName: resourceNames.existingResources.vNet
    volumeMounts: [
      {
        volumeName: 'analytics-file-share-mount'
        mountPath: analyticsFileShareMountPath
      }
      {
        volumeName: 'public-api-file-share-mount'
        mountPath: dataFilesFileShareMountPath
      }
    ]
    volumes: [
      {
        name: 'analytics-file-share-mount'
        storageType: 'AzureFile'
        storageName: resourceNames.existingResources.analyticsFileShare
      }
      {
        name: 'public-api-file-share-mount'
        storageType: 'AzureFile'
        storageName: resourceNames.publicApi.publicApiFileShare
      }
    ]
    appSettings: [
      {
        name: 'ConnectionStrings__PublicDataDb'
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
        name: 'Analytics__Enabled'
        value: 'true'
      }
      {
        name: 'Analytics__BasePath'
        value: analyticsFileShareMountPath
      }
      {
        name: 'App__Url'
        value: publicApiUrl
      }
      {
        name: 'App__EnableSwagger'
        value: enableSwagger ? 'true' : 'false'
      }
      {
        name: 'AppInsights__ConnectionString'
        value: appInsightsConnectionString
      }
      {
        name: 'AzureSearchClient__Endpoint'
        value: searchServiceConfig.endpoint
      }
      {
        name: 'AzureSearchClient__IndexName'
        value: searchServiceConfig.indexName
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
    cpuCores: resourceAndScalingConfig.cpuCores
    memoryGis: resourceAndScalingConfig.memoryGis
    minReplicas: resourceAndScalingConfig.minReplicas
    maxReplicas: resourceAndScalingConfig.maxReplicas
    scaleAtConcurrentHttpRequests: resourceAndScalingConfig.?scaleAtConcurrentHttpRequests
    workloadProfileName: resourceAndScalingConfig.workloadProfileName
    alerts: deployAlerts ? {
      restarts: true
      responseTime: true
      cpuPercentage: true
      memoryPercentage: true
      connectionTimeouts: true
      requestRetries: true
      requestTimeouts: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    tagValues: tagValues
  }
}

module storePublicApiContainerAppPrivateUrl '../../components/keyVaultSecret.bicep' = {
  name: 'storePublicApiContainerAppPrivateUrl'
  params: {
    keyVaultName: resourceNames.existingResources.keyVault
    secretName: 'ees-publicapi-public-api-containerapp-private-url'
    secretValue: 'https://${apiContainerAppModule.outputs.containerAppFqdn}'
  }
}

output containerAppFqdn string = apiContainerAppModule.outputs.containerAppFqdn
output containerAppName string = apiContainerAppModule.outputs.containerAppName
output healthProbePath string = '/health'
