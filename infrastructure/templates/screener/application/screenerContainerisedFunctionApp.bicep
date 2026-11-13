import { FirewallRule, IpRange } from '../../common/types.bicep'
import { FunctionAppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'
import { StorageAccountSku } from '../../common/components/storage/types.bicep'
import { ResourceNames } from '../../common/resource-names.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('SKU of the storage accounts, which sets their redundancy.')
param storageAccountSku StorageAccountSku

@description('Whether the Function App uses its user-assigned managed identity rather than a system-assigned one.')
param useUserAssignedIdentity bool = false

@description('Specifies the location for all resources.')
param location string

@description('Function App Plan : operating system')
param operatingSystem 'Windows' | 'Linux' = 'Linux'

@description('The Application Insights connection string that is associated with this resource.')
param applicationInsightsConnectionString string = ''

param sku FunctionAppServicePlanSku

@description('The IP address ranges that can access the Screener storage accounts.')
param storageFirewallRules IpRange[]

@description('IP address ranges that are allowed to access the Function App endpoints. Dependent on "publicNetworkAccessEnabled" being true.')
param functionAppFirewallRules FirewallRule[] = []

@secure()
@description('Specifies the Application (Client) Id of a pre-existing App Registration used to represent the Screener Function App.')
param screenerAppRegistrationClientId string

@description('Specifies the principal id of the Azure DevOps SPN.')
@secure()
param devopsServicePrincipalId string

@description('The endpoint URL for the core storage blob service.')
param coreStorageBlobEndpoint string

@description('Specifies the login server from the registry.')
@secure()
param acrLoginServer string

@description('Specifies the container image to deploy from the registry.')
param functionAppImageName string

@description('The Docker image tag for the data screener. This value should represent a pipeline build number.')
param screenerDockerImageTag string

@description('Whether or not to include Data Dictionary checks in the Screener.')
param includeDataDictionaryChecks bool

@description('Whether or not to log screening results in the Screener API logs.')
param logScreeningResults bool

@description('Number of concurrent threads that can be used by Plumber to process background jobs.')
param concurrentRWorkers int

@description('The minimum number of instances for the function app.')
param minimumInstanceCount int

@description('The maximum number of instances for the function app - setting to 0 disables the checks on upper scaling limits.')
param maximumInstanceCount int

@description('Whether to create or update Azure Monitor alerts during this deploy.')
param deployAlerts bool

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

var eesyscreenerLogsFileShareMountPath = '/screener/logs'

resource keyVault 'Microsoft.KeyVault/vaults@2026-02-01' existing = {
  name: resourceNames.keyVault.keyVault
}

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.vnet.vnet
}

resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.vnet.subnets.screenerFunctionApp
  parent: vNet
}

resource privateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.vnet.subnets.screenerStoragePrivateEndpoints
  parent: vNet
}

resource adminAppService 'Microsoft.Web/sites@2023-12-01' existing = {
  name: resourceNames.admin.appService
}

resource adminAppServiceIdentity 'Microsoft.ManagedIdentity/identities@2023-01-31' existing = {
  scope: adminAppService
  name: 'default'
}

resource eesyscreenerLogsStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: resourceNames.screener.logsStorageAccount
}

resource screenerIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = if (useUserAssignedIdentity) {
  name: resourceNames.screener.identity
}

// The StartScreening queue trigger connects to the Function App's storage account with a managed identity. With a
// system-assigned identity, the queue URI and credential type are enough. With only a user-assigned identity, the
// trigger must also say which identity to use.
// TODO EES-7502 - remove startScreeningSystemAssignedIdentityAppSettings once every environment has moved to the ELZ,
// leaving only the user-assigned identity settings.
var startScreeningQueueServiceUri = 'https://${resourceNames.screener.functionStorageAccount}.queue.core.windows.net'

var startScreeningSystemAssignedIdentityAppSettings = [
  {
    name: 'AzureWebJobs_StartScreening__queueServiceUri'
    value: startScreeningQueueServiceUri
  }
  {
    name: 'AzureWebJobs_StartScreening__credential'
    value: 'managedidentity'
  }
]

var startScreeningUserAssignedIdentityAppSettings = [
  {
    name: 'AzureWebJobs_StartScreening__queueServiceUri'
    value: startScreeningQueueServiceUri
  }
  {
    name: 'AzureWebJobs_StartScreening__credential'
    value: 'managedidentity'
  }
  {
    name: 'AzureWebJobs_StartScreening__clientId'
    value: screenerIdentity!.properties.clientId
  }
]

var adminAppClientId = adminAppServiceIdentity.properties.clientId
var coreStorageBlobEndpointWithoutTrailingSlash = endsWith(coreStorageBlobEndpoint, '/')
  ? substring(coreStorageBlobEndpoint, 0, length(coreStorageBlobEndpoint) - 1)
  : coreStorageBlobEndpoint

module containerisedFunctionAppModule '../../common/components/function-app/containerised-function-app.bicep' = {
  name: 'screenerContainerisedFunctionAppModuleDeploy'
  params: {
    storageAccountSku: storageAccountSku
    operatingSystem: operatingSystem
    sku: sku
    functionAppName: resourceNames.screener.functionApp
    acrLoginServer: acrLoginServer
    functionAppImageName: functionAppImageName
    functionAppDockerImageTag: screenerDockerImageTag
    location: location
    applicationInsightsConnectionString: applicationInsightsConnectionString
    healthCheckPath: '/api/healthcheck'
    minimumInstanceCount: minimumInstanceCount
    maximumInstanceCount: maximumInstanceCount
    appServicePlanName: resourceNames.screener.appServicePlan
    appSettings: concat([
      {
        name: 'STORAGE_URL'
        value: coreStorageBlobEndpointWithoutTrailingSlash
      }
      {
        name: 'STORAGE_CONTAINER_NAME'
        value: 'releases-temp'
      }
      {
        name: 'LOG_DIR'
        value: eesyscreenerLogsFileShareMountPath
      }
      {
        name: 'DD_CHECKS'
        value: includeDataDictionaryChecks ? 'TRUE' : 'FALSE'
      }
      {
        name: 'LOG_SCREENING_RESULTS'
        value: logScreeningResults ? 'TRUE' : 'FALSE'
      }
      {
          name: 'CONCURRENT_R_WORKERS'
          value: '${concurrentRWorkers}'
      }
    ], useUserAssignedIdentity
      ? startScreeningUserAssignedIdentityAppSettings
      : startScreeningSystemAssignedIdentityAppSettings)
    keyVaultName: keyVault.name
    userAssignedManagedIdentityParams: useUserAssignedIdentity
      ? {
          id: screenerIdentity.id
          name: screenerIdentity.name
          principalId: screenerIdentity!.properties.principalId
          clientId: screenerIdentity!.properties.clientId
        }
      : null
    entraIdAuthentication: {
      appRegistrationClientId: screenerAppRegistrationClientId
      allowedClientIds: [
        adminAppClientId
        devopsServicePrincipalId
      ]
      allowedPrincipalIds: []
      requireAuthentication: true
    }
    dockerPullManagedIdentityClientId: keyVault.getSecret(resourceNames.keyVault.secrets.acr.dockerPullUsername)
    dockerPullManagedIdentitySecretValue: keyVault.getSecret(resourceNames.keyVault.secrets.acr.dockerPullPassword)
    deploymentStorageAccountName: resourceNames.screener.functionStorageAccount
    deploymentStorageAccountConnectionStringSecretName: resourceNames.keyVault.secrets.admin.screenerStorageAccountConnectionString
    functionAppFirewallRules: functionAppFirewallRules
    storageFirewallRules: storageFirewallRules
    publicNetworkAccessEnabled: true
    privateEndpoints: {
      functionApp: privateEndpointSubnet.id
      storageAccounts: privateEndpointSubnet.id
    }
    subnetId: outboundVnetSubnet.id
    includeQueueServices: true
    azureFileShares: [{
      storageName: resourceNames.screener.logsStorageAccount
      storageAccountKey: eesyscreenerLogsStorageAccount.listKeys().keys[0].value
      storageAccountName: resourceNames.screener.logsStorageAccount
      fileShareName: resourceNames.screener.logsFileShare
      mountPath: eesyscreenerLogsFileShareMountPath
    }]
    alerts: deployAlerts
      ? {
          cpuPercentage: true
          memoryPercentage: true
          storageAccountAvailability: true
          storageLatency: false
          alertsGroupName: resourceNames.alertsGroup
          functionAppHealth: true
          httpErrors: true
          fileServiceAvailability: true
          fileServiceCapacity: true
          fileServiceLatency: true
        }
      : null
    tagValues: tagValues
  }
}

output functionAppUrl string = containerisedFunctionAppModule.outputs.url
