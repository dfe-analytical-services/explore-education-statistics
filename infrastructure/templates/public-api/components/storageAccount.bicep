import { FirewallRule } from '../types.bicep'

@description('Specifies the location for all resources.')
param location string

@description('Storage Account Name')
param storageAccountName string

@description('Storage Account Network Rules')
param allowedSubnetIds string[] = []

@description('Storage Account Network Firewall Rules')
param firewallRules FirewallRule[] = []

@description('Storage Account SKU')
param skuStorageResource 'Standard_LRS' | 'Standard_GRS' | 'Standard_RAGRS' | 'Standard_ZRS' | 'Premium_LRS' | 'Premium_ZRS' | 'Standard_GZRS' | 'Standard_RAGZRS' = 'Standard_LRS'

@description('Storage Account Name')
param keyVaultName string

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var endpointSuffix = environment().suffixes.storage

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: skuStorageResource
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
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
