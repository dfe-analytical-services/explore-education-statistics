import { ResourceNames, FirewallRule, IpRange } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('The Application Insights key that is associated with this resource')
param applicationInsightsKey string

@description('Specifies whether or not the Data Processor Function App already exists.')
param dataProcessorFunctionAppExists bool

@description('Specifies the Application (Client) Id of a pre-existing App Registration used to represent the Data Processor Function App.')
param dataProcessorAppRegistrationClientId string

@description('Specifies the principal id of the Azure DevOps SPN.')
@secure()
param devopsServicePrincipalId string

@description('The IP address ranges that can access the Data Processor storage accounts.')
param storageFirewallRules IpRange[]

@description('The IP address ranges that can access the Data Processor Function App endpoints.')
param functionAppFirewallRules FirewallRule[]

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var publicApiDataFileShareMountPath = '/data/public-api-data'

resource adminAppService 'Microsoft.Web/sites@2023-12-01' existing = {
  name: resourceNames.existingResources.adminApp
}

resource adminAppServiceIdentity 'Microsoft.ManagedIdentity/identities@2023-01-31' existing = {
  scope: adminAppService
  name: 'default'
}

var adminAppClientId = adminAppServiceIdentity.properties.clientId

resource publicApiStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: resourceNames.publicApi.publicApiStorageAccount
}

resource dataProcessorFunctionAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: resourceNames.publicApi.dataProcessorIdentity
  location: location
}

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.dataProcessor
  parent: vNet
}

resource inboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.dataProcessorPrivateEndpoints
  parent: vNet
}

// Deploy Data Processor Function.
module dataProcessorFunctionAppModule '../../components/functionApp.bicep' = {
  name: 'dataProcessorFunctionAppDeploy'
  params: {
    functionAppName: resourceNames.publicApi.dataProcessor
    appServicePlanName: resourceNames.publicApi.dataProcessor
    storageAccountsNamePrefix: resourceNames.publicApi.dataProcessorStorageAccountsPrefix
    location: location
    applicationInsightsKey: applicationInsightsKey
    subnetId: outboundVnetSubnet.id
    privateEndpointSubnetId: inboundVnetSubnet.id
    publicNetworkAccessEnabled: true
    functionAppFirewallRules: functionAppFirewallRules
    entraIdAuthentication: {
      appRegistrationClientId: dataProcessorAppRegistrationClientId
      allowedClientIds: [
        adminAppClientId
        devopsServicePrincipalId
      ]
      allowedPrincipalIds: []
      requireAuthentication: true
    }
    userAssignedManagedIdentityParams: {
      id: dataProcessorFunctionAppManagedIdentity.id
      name: dataProcessorFunctionAppManagedIdentity.name
      principalId: dataProcessorFunctionAppManagedIdentity.properties.principalId
    }
    functionAppExists: dataProcessorFunctionAppExists
    keyVaultName: resourceNames.existingResources.keyVault
    functionAppRuntime: 'dotnet-isolated'
    sku: {
      name: 'EP1'
      tier: 'ElasticPremium'
      family: 'EP'
    }
    preWarmedInstanceCount: 1
    healthCheckPath: '/api/HealthCheck'
    azureFileShares: [{
      storageName: resourceNames.publicApi.publicApiFileShare
      storageAccountKey: publicApiStorageAccount.listKeys().keys[0].value
      storageAccountName: resourceNames.publicApi.publicApiStorageAccount
      fileShareName: resourceNames.publicApi.publicApiFileShare
      mountPath: publicApiDataFileShareMountPath
    }]
    storageFirewallRules: storageFirewallRules
    tagValues: tagValues
  }
}

module functionAppHealthAlert '../../components/alerts/sites/healthAlert.bicep' = if (deployAlerts) {
  name: '${resourceNames.publicApi.dataProcessor}HealthDeploy'
  params: {
    resourceNames: [resourceNames.publicApi.dataProcessor]
    alertsGroupName: resourceNames.existingResources.alertsGroup
    tagValues: tagValues
  }
}

module storageAccountAvailabilityAlerts '../../components/alerts/storageAccounts/availabilityAlert.bicep' = if (deployAlerts) {
  name: '${resourceNames.publicApi.dataProcessor}StorageAvailabilityDeploy'
  params: {
    resourceNames: [
      dataProcessorFunctionAppModule.outputs.managementStorageAccountName
      dataProcessorFunctionAppModule.outputs.slot1StorageAccountName
      dataProcessorFunctionAppModule.outputs.slot2StorageAccountName
    ]
    alertsGroupName: resourceNames.existingResources.alertsGroup
    tagValues: tagValues
  }
}

module fileServiceAvailabilityAlerts '../../components/alerts/fileServices/availabilityAlert.bicep' = if (deployAlerts) {
  name: '${resourceNames.publicApi.dataProcessor}FsAvailabilityDeploy'
  params: {
    resourceNames: [
      dataProcessorFunctionAppModule.outputs.managementStorageAccountName
      dataProcessorFunctionAppModule.outputs.slot1StorageAccountName
      dataProcessorFunctionAppModule.outputs.slot2StorageAccountName
    ]
    alertsGroupName: resourceNames.existingResources.alertsGroup
    tagValues: tagValues
  }
}

module storageAccountLatencyAlert '../../components/alerts/storageAccounts/latencyAlert.bicep' = if (deployAlerts) {
  name: '${resourceNames.publicApi.publicApiStorageAccount}LatencyDeploy'
  params: {
    resourceNames: [
      dataProcessorFunctionAppModule.outputs.managementStorageAccountName
      dataProcessorFunctionAppModule.outputs.slot1StorageAccountName
      dataProcessorFunctionAppModule.outputs.slot2StorageAccountName
    ]
    alertsGroupName: resourceNames.existingResources.alertsGroup
    tagValues: tagValues
  }
}

module fileServiceLatencyAlert '../../components/alerts/fileServices/latencyAlert.bicep' = if (deployAlerts) {
  name: '${resourceNames.publicApi.publicApiStorageAccount}FsLatencyDeploy'
  params: {
    resourceNames: [
      dataProcessorFunctionAppModule.outputs.managementStorageAccountName
      dataProcessorFunctionAppModule.outputs.slot1StorageAccountName
      dataProcessorFunctionAppModule.outputs.slot2StorageAccountName
    ]
    alertsGroupName: resourceNames.existingResources.alertsGroup
    tagValues: tagValues
  }
}

output managedIdentityName string = dataProcessorFunctionAppManagedIdentity.name
output managedIdentityClientId string = dataProcessorFunctionAppManagedIdentity.properties.clientId
output publicApiDataFileShareMountPath string = publicApiDataFileShareMountPath
output url string = dataProcessorFunctionAppModule.outputs.url
output stagingUrl string = dataProcessorFunctionAppModule.outputs.stagingUrl
