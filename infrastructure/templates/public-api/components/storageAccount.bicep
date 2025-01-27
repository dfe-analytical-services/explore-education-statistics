import { responseTimeConfig } from 'alerts/dynamicAlertConfig.bicep'
import { staticAverageLessThanHundred, staticAverageGreaterThanZero } from 'alerts/staticAlertConfig.bicep'
import { IpRange, StorageAccountPrivateEndpoints } from '../types.bicep'

@description('Specifies the location for all resources.')
param location string

@description('Storage Account Name')
param storageAccountName string

@description('Storage Account Network Rules')
param allowedSubnetIds string[] = []

@description('Storage Account Network Firewall Rules')
param firewallRules IpRange[] = []

@description('Storage Account SKU')
param sku 'Standard_LRS' | 'Standard_GRS' | 'Standard_RAGRS' | 'Standard_ZRS' | 'Premium_LRS' | 'Premium_ZRS' | 'Standard_GZRS' | 'Standard_RAGZRS' = 'Standard_LRS'

@description('Storage Account kind')
param kind 'StorageV2' | 'FileStorage' = 'StorageV2'

@description('Storage Account Name')
param keyVaultName string

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

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: kind
  sku: {
    name: sku
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: publicNetworkAccessEnabled ? 'Enabled' : 'Disabled'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Deny'
      ipRules: [for firewallRule in firewallRules: {
        value: firewallRule.cidr
        action: 'Allow'
      }]
      virtualNetworkRules: [for subnetId in allowedSubnetIds: {
        #disable-next-line use-resource-id-functions
        id: subnetId
        action: 'Allow'
      }]
    }
  }
  tags: tagValues
}

module fileServicePrivateEndpointModule 'privateEndpoint.bicep' = if (privateEndpointSubnetIds.?file != '') {
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

module blobStoragePrivateEndpointModule 'privateEndpoint.bicep' = if (privateEndpointSubnetIds.?blob != '') {
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

module queuePrivateEndpointModule 'privateEndpoint.bicep' = if (privateEndpointSubnetIds.?queue != '') {
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

module tableStoragePrivateEndpointModule 'privateEndpoint.bicep' = if (privateEndpointSubnetIds.?table != '') {
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

module availabilityAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.availability) {
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

module latencyAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.latency) {
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

var connectionStringSecretName = '${storageAccountName}-connection-string'

module storeADOConnectionStringToKeyVault './keyVaultSecret.bicep' = {
  name: '${storageAccountName}ConnectionStringSecretDeploy'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretValue: storageAccountConnectionString
    contentType: 'text/plain'
    secretName: connectionStringSecretName
  }
}

var accessKeySecretName = '${storageAccountName}-access-key'

module storeAccessKeyToKeyVault './keyVaultSecret.bicep' = {
  name: '${storageAccountName}AccessKeySecretDeploy'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretValue: key
    contentType: 'text/plain'
    secretName: accessKeySecretName
  }
}

output storageAccountName string = storageAccount.name
output connectionStringSecretName string = connectionStringSecretName
output accessKeySecretName string = accessKeySecretName
