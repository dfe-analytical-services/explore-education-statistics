@description('The name of the backup policy to register the file share with.')
param backupPolicyName string

@description('The name of the owning Recovery Services Vault.')
param vaultName string

@description('The name of the storage account that the file share belongs to.')
param storageAccountName string

@description('The name of the file share being registered with the backup policy.')
param fileShareName string

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var backupPolicyId = resourceId(
  resourceGroup().name,
  'Microsoft.RecoveryServices/vaults/backupPolicies',
  vaultName,
  backupPolicyName
)

var storageAccountId = resourceId(resourceGroup().name, 'Microsoft.Storage/storageAccounts', storageAccountName)

resource protectionContainer 'Microsoft.RecoveryServices/vaults/backupFabrics/protectionContainers@2021-12-01' = {
  name: '${vaultName}/Azure/storagecontainer;Storage;${resourceGroup().name};${storageAccountName}'
  properties: {
    backupManagementType: 'AzureStorage'
    containerType: 'StorageContainer'
    sourceResourceId: storageAccountId
  }
  tags: tagValues
}

resource protectedItem 'Microsoft.RecoveryServices/vaults/backupFabrics/protectionContainers/protectedItems@2021-12-01' = {
  parent: protectionContainer
  name: 'AzureFileShare;${fileShareName}'
  properties: {
    protectedItemType: 'AzureFileShareProtectedItem'
    sourceResourceId: storageAccountId
    policyId: backupPolicyId
  }
  tags: tagValues
}
