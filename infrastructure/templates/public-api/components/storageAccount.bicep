@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Storage Account Name')
param storageAccountName string

@description('Storage Account Network Rules')
param allowedSubnetIds string[] = []

@description('Storage Account Network Firewall Rules')
param storageFirewallRules array = []

@description('Storage Account SKU')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
  'Premium_ZRS'
  'Standard_GZRS'
  'Standard_RAGZRS'
])
param skuStorageResource string = 'Standard_LRS'

@description('Storage Account Name')
param keyVaultName string

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

// Variables and created data
var endpointSuffix = environment().suffixes.storage

//Resources 
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
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
      ipRules:  [for ipRule in storageFirewallRules: {
        value: ipRule
        action: 'Allow'
      }]
      virtualNetworkRules: [for ipRule in allowedSubnetIds: {
        id: ipRule
        action: 'Allow'
      }]
    }
  }
  tags: tagValues
}

var key = storageAccount.listKeys().keys[0].value
var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${endpointSuffix};AccountKey=${key}'

var connectionStringSecretName = '${storageAccountName}-connectionString'

module storeADOConnectionStringToKeyVault './keyVaultSecret.bicep' = {
  name: 'saConnectionStringSecretDeploy'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretValue: storageAccountConnectionString 
    contentType: 'text/plain'
    secretName: connectionStringSecretName
  }
}

var accessKeySecretName = '${storageAccountName}-connectionString'

module storeAccessKeyToKeyVault './keyVaultSecret.bicep' = {
  name: 'saAccessKeySecretDeploy'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretValue: key
    contentType: 'text/plain'
    secretName: accessKeySecretName
  }
}

//Outputs
output storageAccountName string = storageAccount.name
output connectionStringSecretName string = connectionStringSecretName
output accessKeySecretName string = accessKeySecretName
