import { abbreviations } from '../../../common/abbreviations.bicep'
import { IpRange } from '../../../common/types.bicep'
import { VNetSubnets } from '../virtual-network/types.bicep'

@description('Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string

@description('Retention of blobs in days.')
param blobDeleteRetentionDays int

@description('Firewall rules.')
param firewallRules IpRange[]

@description('Name of the Key Vault in which to store the storage account secrets.')
param keyVaultName string

@description('Name of the Action Group that receives alerts.')
param alertsGroupName string

@description('Subnets from the VNet.')
param subnets VNetSubnets

@description('The name of the Backup Vault instance used to back up this storage account.')
param backupVaultName string

@description('The name of the Backup Vault policy used to back up blobs.')
param backupBlobsPolicyName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Tags for the resources')
param tagValues object

resource backupVault 'Microsoft.DataProtection/backupVaults@2026-06-01' existing = {
  name: backupVaultName
}

var storageAccountName = '${subscription}${abbreviations.storageStorageAccounts}eespublic'

// TODO EES-7502 - remove when standardising role assignment GUID generation.
var backupContributorRoleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'e5e2a7ff-d759-4cd2-bb51-3152d37e2eb1') 

module storageAccountModule '../../../common/components/storage/storage-account.bicep' = {
  name: 'publicStorageAccountDeploy'
  params: {
    storageAccountName: storageAccountName
    kind: 'StorageV2'
    sku: 'Standard_LRS'
    publicNetworkAccessEnabled: true
    keyVaultName: keyVaultName
    connectionStringSecretNameOverride: 'ees-storage-public'
    firewallRules: firewallRules
    allowedSubnetIds: [
      subnets.admin.id
      subnets.content.id
      subnets.data.id
      subnets.publisher.id
    ]
    alerts: deployAlerts ? {
      alertsGroupName: alertsGroupName
      availability: true
      latency: false
    } : null
    tagValues: tagValues
  }
}

module blobServiceModule '../../../common/components/blobService.bicep' = {
  name: 'publicStorageAccountBlobServiceDeploy'
  params: {
    storageAccountName: storageAccountModule.outputs.storageAccountName
    deleteRetentionPolicy: blobDeleteRetentionDays
  }
}

module backupVaultRoleAssignmentModule '../../../common/components/storageAccountRoleAssignment.bicep' = {
  name: 'publicStorageBackupVaultRoleAssignmentModuleDeploy'
  params: {
    storageAccountName: storageAccountModule.outputs.storageAccountName
    principalIds: [backupVault.identity.principalId]
    role: 'Storage Account Backup Contributor'
    roleAssignmentNameOverride: guid(backupVault.id, backupContributorRoleDefinitionId, storageAccountModule.outputs.storageAccountId)
  }
}

module backupVaultRegistration '../../../common/components/data-protection/blobs-backup-vault-instance.bicep' = {
  name: 'publicStorageBackupVaultRegistrationModuleDeploy'
  params: {
    vaultName: backupVault.name
    instanceName: storageAccountName
    backupPolicyName: backupBlobsPolicyName
    resourceId: storageAccountModule.outputs.storageAccountId
    resourceLocation: resourceGroup().location
    excludedContainerPrefixes: ['cache']
    tagValues: tagValues
  }
  dependsOn: [
    blobServiceModule
    backupVaultRoleAssignmentModule
  ]
}
