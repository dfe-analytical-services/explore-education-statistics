import { abbreviations } from '../../common/abbreviations.bicep'
import { IpRange, FirewallRule } from '../../common/types.bicep'
import { ResourceNames } from '../types.bicep'

@description('The IP address ranges that can access the Analytics Function App endpoints.')
param functionAppFirewallRules FirewallRule[]

@description('The IP address ranges that can access the Search Docs Function App storage accounts.')
param storageFirewallRules IpRange[]

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Location for all resources.')
param location string

@description('Name of the shared analytics storage account.')
param analyticsStorageAccountName string

@description('Name of the shared analytics file share.')
param analyticsFileShareName string

@description('The Application Insights connection string that is associated with this resource.')
param applicationInsightsConnectionString string = ''

@description('The cron schedule for the Public API queries consumer Function. Defaults to every half hour.')
param publicApiQueryConsumerCron string

@description('Specifies whether or not the Analytics Function App already exists.')
param functionAppExists bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.existingResources.keyVault
}

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}
resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.analyticsFunctionApp
  parent: vNet
}

resource analyticsStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: analyticsStorageAccountName
}

resource storagePrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.storagePrivateEndpoints
  parent: vNet
}

var fileShareMountPath = '/data/analytics'

module functionAppModule '../../common/components/functionApp.bicep' = {
  name: 'analyticsFunctionAppModuleDeploy'
  params: {
    functionAppName: '${resourcePrefix}-${abbreviations.webSitesFunctions}-analytics'
    location: location
    applicationInsightsConnectionString: applicationInsightsConnectionString
    appServicePlanName: '${resourcePrefix}-${abbreviations.webServerFarms}-analytics'
    appSettings: [
      {
        name: 'Analytics__BasePath'
        value: fileShareMountPath
      }
      {
        name: 'App__ConsumePublicApiQueriesCronSchedule'
        value: publicApiQueryConsumerCron
      }
    ]
    functionAppExists: functionAppExists
    keyVaultName: keyVault.name
    sku: {
      name: 'EP1'
      tier: 'ElasticPremium'
      family: 'EP'
    }
    healthCheckPath: '/api/HealthCheck'
    operatingSystem: 'Linux'
    functionAppRuntime: 'dotnet-isolated'
    functionAppRuntimeVersion: '8.0'
    storageAccountName: '${replace(resourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}anlytfa'
    storageAccountPublicNetworkAccessEnabled: false
    publicNetworkAccessEnabled: true
    functionAppFirewallRules: functionAppFirewallRules
    storageFirewallRules: storageFirewallRules
    outboundSubnetId: outboundVnetSubnet.id
    privateEndpoints: {
      storageAccounts: storagePrivateEndpointSubnet.id
    }
    alerts: {
      cpuPercentage: true
      functionAppHealth: true
      httpErrors: true
      memoryPercentage: true
      storageAccountAvailability: false
      storageLatency: false
      fileServiceAvailability: false
      fileServiceLatency: false
      fileServiceCapacity: false
      alertsGroupName: resourceNames.existingResources.alertsGroup
    }
    azureFileShares: [{
      storageName: analyticsStorageAccountName
      storageAccountKey: analyticsStorageAccount.listKeys().keys[0].value
      storageAccountName: analyticsStorageAccountName
      fileShareName: analyticsFileShareName
      mountPath: fileShareMountPath
    }]
    tagValues: tagValues
  }
}

output functionAppName string = functionAppModule.outputs.name
output functionAppUrl string = functionAppModule.outputs.url
