import { staticAverageLessThanHundred, staticAverageGreaterThanZero } from '../alerts/staticAlertConfig.bicep'
import { StorageAccountPrivateEndpoints, StorageAccountKind, StorageAccountSku } from 'types.bicep'
import { IpRange } from '../../types.bicep'

@description('Specifies the location for all resources.  Defaults to the Resource Group location.')
param location string = resourceGroup().location

@description('Storage Account Name')
param storageAccountName string

@description('Storage Account Network Rules')
param allowedSubnetIds string[] = []

@description('Storage Account Network Firewall Rules')
param firewallRules IpRange[] = []

@description('Storage Account SKU')
param sku StorageAccountSku = 'Standard_LRS'

@description('Storage Account kind')
param kind StorageAccountKind = 'StorageV2'

@description('The access tier for Blob access.')
param accessTier 'Hot' | 'Cool' | 'Cold' | 'Premium' = 'Hot'

@description('Key Vault Name.  If specified, a Key Vault secret will be added for this storage account connection string.')
param keyVaultName string?

@description('The name of the Key Vault secret holding the connection string for this storage account. Defaults to "<storageAccountName>-connection-string" if not supplied.')
param connectionStringSecretNameOverride string?

@description('Whether the storage account is accessible from the public internet')
param publicNetworkAccessEnabled bool = false

@description('Private endpoint subnets')
param privateEndpointSubnetIds StorageAccountPrivateEndpoints?

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  availability: bool
  latency: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var endpointSuffix = environment().suffixes.storage

var deployNetworkAccessRestrictions = publicNetworkAccessEnabled && (length(firewallRules) > 0 || length(allowedSubnetIds) > 0)

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: kind
  sku: {
    name: sku
  }
  properties: {
    accessTier: accessTier
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: publicNetworkAccessEnabled ? 'Enabled' : 'Disabled'
    networkAcls: deployNetworkAccessRestrictions ? {
      bypass: 'AzureServices'
      defaultAction: 'Deny'
      ipRules: map(firewallRules, firewallRule => {
        value: firewallRule.cidr
        action: 'Allow'
      })
      virtualNetworkRules: map(allowedSubnetIds, subnetId => {
        #disable-next-line use-resource-id-functions
        id: subnetId
        action: 'Allow'
      })
    } : null
  }
  tags: tagValues
}

module fileServicePrivateEndpointModule '../privateEndpoint.bicep' = if (privateEndpointSubnetIds.?file != null) {
  name: '${storageAccountName}FileServicePrivateEndpointDeploy'
  params: {
    serviceId: storageAccount.id
    serviceName: storageAccount.name
    privateEndpointNameOverride: '${storageAccount.name}-file'
    serviceType: 'fileService'
    subnetId: privateEndpointSubnetIds.?file ?? ''
    location: location
    tagValues: tagValues
  }
}

module blobStoragePrivateEndpointModule '../privateEndpoint.bicep' = if (privateEndpointSubnetIds.?blob != null) {
  name: '${storageAccountName}BlobStoragePrivateEndpointDeploy'
  params: {
    serviceId: storageAccount.id
    serviceName: storageAccount.name
    privateEndpointNameOverride: '${storageAccount.name}-blob'
    serviceType: 'blobStorage'
    subnetId: privateEndpointSubnetIds.?blob ?? ''
    location: location
    tagValues: tagValues
  }
}

module queuePrivateEndpointModule '../privateEndpoint.bicep' = if (privateEndpointSubnetIds.?queue != null) {
  name: '${storageAccountName}QueuePrivateEndpointDeploy'
  params: {
    serviceId: storageAccount.id
    serviceName: storageAccount.name
    privateEndpointNameOverride: '${storageAccount.name}-queue'
    serviceType: 'queue'
    subnetId: privateEndpointSubnetIds.?queue ?? ''
    location: location
    tagValues: tagValues
  }
}

module tableStoragePrivateEndpointModule '../privateEndpoint.bicep' = if (privateEndpointSubnetIds.?table != null) {
  name: '${storageAccountName}TableStoragePrivateEndpointDeploy'
  params: {
    serviceId: storageAccount.id
    serviceName: storageAccount.name
    privateEndpointNameOverride: '${storageAccount.name}-table'
    serviceType: 'tableStorage'
    subnetId: privateEndpointSubnetIds.?table ?? ''
    location: location
    tagValues: tagValues
  }
}

module availabilityAlert '../alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.availability) {
  name: '${storageAccountName}AvailabilityAlertModule'
  params: {
    resourceName: storageAccountName
    resourceMetric: {
      resourceType: 'Microsoft.Storage/storageAccounts'
      metric: 'availability'
    }
    config: {
      ...staticAverageLessThanHundred
      nameSuffix: 'availability'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module latencyAlert '../alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.latency) {
  name: '${storageAccountName}LatencyDeploy'
  params: {
    resourceName: storageAccountName
    resourceMetric: {
      resourceType: 'Microsoft.Storage/storageAccounts'
      metric: 'SuccessE2ELatency'
    }
    config: {
      ...staticAverageGreaterThanZero
      nameSuffix: 'response-time'
      threshold: '250'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

var key = storageAccount.listKeys().keys[0].value
var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${endpointSuffix};AccountKey=${key}'

// TODO EES-7502 - make secret names consistent.
var connectionStringSecretName = connectionStringSecretNameOverride ?? '${storageAccountName}-connection-string'

module storeADOConnectionStringToKeyVault '../key-vault/keyVaultSecret.bicep' = if (keyVaultName != null) {
  name: '${storageAccountName}ConnectionStringSecretDeploy'
  params: {
    keyVaultName: keyVaultName!
    secretName: connectionStringSecretName
    secretValue: storageAccountConnectionString
  }
}

var accessKeySecretName = '${storageAccountName}-access-key'

module storeAccessKeyToKeyVault '../key-vault/keyVaultSecret.bicep' = if (keyVaultName != null) {
  name: '${storageAccountName}AccessKeySecretDeploy'
  params: {
    keyVaultName: keyVaultName!
    secretValue: key
    secretName: accessKeySecretName
  }
}

output storageAccountId string = storageAccount.id
output storageAccountName string = storageAccount.name
output connectionStringSecretName string = connectionStringSecretName
output accessKeySecretName string = accessKeySecretName
