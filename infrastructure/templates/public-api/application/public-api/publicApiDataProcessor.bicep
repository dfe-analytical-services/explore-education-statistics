import { ResourceNames, FirewallRule } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Alert metric name prefix')
param metricsNamePrefix string

@description('The Application Insights key that is associated with this resource')
param applicationInsightsKey string

@description('Specifies whether or not the Data Processor Function App already exists.')
param dataProcessorFunctionAppExists bool = false

@description('Specifies the Application (Client) Id of a pre-existing App Registration used to represent the Data Processor Function App.')
param dataProcessorAppRegistrationClientId string

@description('Public API Storage : Firewall rules.')
param storageFirewallRules FirewallRule[] = []

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
var adminAppPrincipalId = adminAppServiceIdentity.properties.principalId

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
    alertsGroupName: resourceNames.existingResources.alertsGroup
    location: location
    applicationInsightsKey: applicationInsightsKey
    subnetId: outboundVnetSubnet.id
    privateEndpointSubnetId: inboundVnetSubnet.id
    publicNetworkAccessEnabled: false
    entraIdAuthentication: {
      appRegistrationClientId: dataProcessorAppRegistrationClientId
      allowedClientIds: [
        adminAppClientId
      ]
      allowedPrincipalIds: [
        adminAppPrincipalId
      ]
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
    healthCheck: {
      path: '/api/HealthCheck'
      unhealthyMetricName: '${metricsNamePrefix}Unhealthy'
    }
    appSettings: {
      App__MetaInsertBatchSize: 1000
    }
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

output managedIdentityName string = dataProcessorFunctionAppManagedIdentity.name
output managedIdentityClientId string = dataProcessorFunctionAppManagedIdentity.properties.clientId
output publicApiDataFileShareMountPath string = publicApiDataFileShareMountPath
