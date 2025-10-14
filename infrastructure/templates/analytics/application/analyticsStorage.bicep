import { abbreviations } from '../../common/abbreviations.bicep'
import { ResourceNames } from '../types.bicep'
import { IpRange } from '../../common/types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

@description('Firewall rules.')
param storageFirewallRules IpRange[]

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource storagePrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.storagePrivateEndpoints
  parent: vNet
}

var storageAccountName = '${replace(resourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}anlyt'

var storageAccountConfig = {
  kind: 'FileStorage'
  sku: 'Premium_ZRS'
  fileShare: {
    quotaGbs: 100
    accessTier: 'Premium'
  }
}

var fileShareName = '${resourcePrefix}-${abbreviations.fileShare}-anlyt'

module analyticsStorageAccountModule '../../public-api/components/storageAccount.bicep' = {
  name: 'analyticsStorageAccountDeploy'
  params: {
    location: location
    storageAccountName: storageAccountName
    publicNetworkAccessEnabled: true
    firewallRules: storageFirewallRules
    sku: storageAccountConfig.sku
    kind: storageAccountConfig.kind
    keyVaultName: resourceNames.existingResources.keyVault
    alerts: deployAlerts ? {
      availability: true
      latency: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    privateEndpointSubnetIds: {
      file: storagePrivateEndpointSubnet.id
    }
    tagValues: tagValues
  }
}

module analyticsFileShareModule '../../public-api/components/fileShare.bicep' = {
  name: 'analyticsFileShareDeploy'
  params: {
    fileShareName: fileShareName
    fileShareQuotaGbs: storageAccountConfig.fileShare.quotaGbs
    storageAccountName: analyticsStorageAccountModule.outputs.storageAccountName
    fileShareAccessTier: storageAccountConfig.fileShare.accessTier
    alerts: deployAlerts ? {
      availability: true
      latency: true
      capacity: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    tagValues: tagValues
  }
}

output storageAccountName string = analyticsStorageAccountModule.outputs.storageAccountName
output fileShareName string = fileShareName
