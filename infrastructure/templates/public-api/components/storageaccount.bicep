@description('Specifies the Subscription to be used.')
param subscription string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Storage : Storage Account Name')
param storageAccountName string = 'saeespublicapi'

@description('Storage : Storage Account Subnet')
param adminSubnetRef string

@description('Storage : Storage Account Subnet')
param importerSubnetRef string

@description('Storage : Storage Account Subnet')
param publisherSubnetRef string

@description('Storage : Storage Account Subnet')
param storageFirewallRules array = []

@description('Storage : Storage Account SKU')
@allowed(['Standard_LRS','Standard_GRS','Standard_RAGRS','Standard_ZRS','Premium_LRS','Premium_ZRS','Standard_GZRS','Standard_RAGZRS'])
param skuStorageResource string = 'Standard_LRS'

//Passed in Tags
param departmentName string = 'Public API'
param environmentName string = 'Development'
param solutionName string = 'API'
param subscriptionName string = 'Unknown'
param costCentre string = 'Unknown'
param serviceOwnerName string = 'Unknown'
param dateProvisioned string = utcNow('u')
param createdBy string = 'Unknown'
param deploymentRepo string = 'N/A'
param deploymentScript string = 'N/A'


// Variables and created data
var publicAPIStorageAccountName = '${subscription}${storageAccountName}'
var endpointSuffix = environment().suffixes.storage
var key = publicAPIStorageAccount.listKeys().keys[0].value

//Resources 
resource publicAPIStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: publicAPIStorageAccountName
  location: location
  kind: 'StorageV2' //filestorage
  sku: {
    name: skuStorageResource //Premium_LRS
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
      virtualNetworkRules: [
        {
          id: adminSubnetRef
          action: 'Allow'
        }
        {
          id: importerSubnetRef
          action: 'Allow'
        }
        {
          id: publisherSubnetRef
          action: 'Allow'
        }
      ]
    }
  }
  tags: {
    Department: departmentName
    Solution: solutionName
    ServiceType: 'Storage Account'
    Environment: environmentName
    Subscription: subscriptionName
    CostCentre: costCentre
    ServiceOwner: serviceOwnerName
    DateProvisioned: dateProvisioned
    CreatedBy: createdBy
    DeploymentRepo: deploymentRepo
    DeploymentScript: deploymentScript
  }
}

//Outputs
output storageAccountName string = publicAPIStorageAccount.name
output storageAccountConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${publicAPIStorageAccountName};EndpointSuffix=${endpointSuffix};AccountKey=${key}'







