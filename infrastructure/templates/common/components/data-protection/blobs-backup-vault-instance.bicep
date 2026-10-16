import { getFullBackupVaultDataSourceType } from 'functions.bicep'

@description('Full resource id of the resource instance that owns the data source.')
param resourceId string

@description('The location of the resource being backed up.')
param resourceLocation string

@description('Full resource id of the backup policy that is used to backup this resource.')
param backupPolicyName string

@description('Name of the backup vault that this policy belongs to.')
param vaultName string

@description('Name of the backup instance to create, unique within the vault.')
param instanceName string

@description('An array of container name prefixes to exclude from backups.')
param excludedContainerPrefixes string[] = []

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

resource policy 'Microsoft.DataProtection/backupVaults/backupPolicies@2026-06-01' existing = {
  name: '${vaultName}/${backupPolicyName}'
}

var dataSourceInfo = {
  objectType: 'Datasource'
  resourceID: resourceId
  resourceName: last(split(resourceId, '/'))
  resourceType: 'Microsoft.Storage/storageAccounts'
  resourceUri: resourceId
  resourceLocation: resourceLocation
  datasourceType: getFullBackupVaultDataSourceType('blobs')
}

resource backupInstance 'Microsoft.DataProtection/backupVaults/backupInstances@2026-06-01' = {
  name: '${vaultName}/${instanceName}'
  properties: {
    objectType: 'BackupInstance'
    friendlyName: instanceName
    dataSourceInfo: dataSourceInfo
    dataSourceSetInfo: union(dataSourceInfo, {
      objectType: 'DatasourceSet'
    })
    policyInfo: {
      policyId: policy.id
      name: backupPolicyName
      policyParameters: {
        backupDatasourceParametersList: [
          {
            objectType: 'BlobBackupDatasourceParametersForAutoProtection'
            autoProtectionSettings: {
              objectType: 'BlobBackupRuleBasedAutoProtectionSettings'
              enabled: true
              rules: map(excludedContainerPrefixes, prefix => 
                {
                  objectType: 'BlobBackupAutoProtectionRule'
                  mode: 'Exclude'
                  type: 'Prefix'
                  pattern: prefix
                }
              )
            }
          }
        ]
      }
    }
  }
  tags: tagValues
}
