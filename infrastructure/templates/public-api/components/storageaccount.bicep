@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Storage Account Name')
param storageAccountName string

@description('Storage Account Network Rules')
param storageSubnetRules array = []

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

//Passed in Tags
param tagValues object

// Variables and created data
var storageName = replace('${resourcePrefix}sacc${storageAccountName}', '-', '')
var endpointSuffix = environment().suffixes.storage
var key = storageAccount.listKeys().keys[0].value

//Resources 
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageName
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
      virtualNetworkRules: [for ipRule in storageSubnetRules: {
        id: ipRule
        action: 'Allow'
      }]
    }
  }
  tags: tagValues
}

//Outputs
output storageAccountName string = storageAccount.name
output storageAccountConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${endpointSuffix};AccountKey=${key}'
