import { IpRange, FirewallRule } from '../../common/types.bicep'
import { ResourceNames } from '../types.bicep'

@description('The IP address ranges that can access the Search Docs Function App endpoints.')
param functionAppFirewallRules FirewallRule[]

@description('The IP address ranges that can access the Search Docs Function App storage accounts.')
param storageFirewallRules IpRange[]

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Location for all resources.')
param location string

@description('The Application Insights key that is associated with this resource')
param applicationInsightsKey string = ''

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.searchDocsFunction
  parent: vNet
}

resource searchDocsFunctionPrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.searchDocsFunctionPrivateEndpoints
  parent: vNet
}

module functionAppModule '../../common/components/functionApp.bicep' = {
  name: 'functionAppModuleDeploy'
  params: {
    functionAppName: resourceNames.search.searchDocsFunction
    location: location
    applicationInsightsKey: applicationInsightsKey
    appServicePlanName: resourceNames.search.searchDocsFunction
    keyVaultName: resourceNames.existingResources.keyVault
    sku: {
      name: 'FC1'
      tier: 'FlexConsumption'
    }
    operatingSystem: 'Linux'
    functionAppRuntime: 'dotnet-isolated'
    functionAppRuntimeVersion: '8.0'
    storageAccountName: resourceNames.search.searchDocsFunctionStorageAccount
    storageAccountPublicNetworkAccessEnabled: true
    publicNetworkAccessEnabled: true
    functionAppFirewallRules: functionAppFirewallRules
    storageFirewallRules: storageFirewallRules
    privateEndpoints: {
      functionApp: searchDocsFunctionPrivateEndpointSubnet.id
      storageAccounts: searchDocsFunctionPrivateEndpointSubnet.id
    }
    subnetId: outboundVnetSubnet.id
    alerts: deployAlerts ? {
      cpuPercentage: true
      memoryPercentage: true
      storageAccountAvailability: true
      storageLatency: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    tagValues: tagValues
  }
}
