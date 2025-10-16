import { BackupVaultPolicyDataSourceType, getFullBackupVaultDataSourceType } from '../../types.bicep'

@description('Type of data source to be backed up.')
param dataSourceType BackupVaultPolicyDataSourceType

@description('Full resource id of the resource instance that owns the data source.')
param resourceId string

@description('Full resource id of the backup policy that is used to backup this resource.')
param backupPolicyName string

@description('Name of the backup vault that this policy belongs to.')
param vaultName string

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

resource policy 'Microsoft.DataProtection/backupVaults/backupPolicies@2022-05-01' existing = {
  name: backupPolicyName
}

resource backupInstance 'Microsoft.DataProtection/backupVaults/backupInstances@2023-01-01' = {
  name: '${vaultName}/PostgreSQLBackupInstance'
  properties: {
    dataSourceInfo: {
      datasourceType: getFullBackupVaultDataSourceType(dataSourceType)
      objectType: 'Datasource'
      resourceID: resourceId
    }
    policyInfo: {
      policyId: policy.id
    }
    objectType: 'BackupInstance'
  }
  tags: tagValues
}
