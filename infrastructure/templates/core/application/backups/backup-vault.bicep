import { abbreviations } from '../../../common/abbreviations.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Whether or not to create role assignments necessary for performing certain backup actions.')
param deployBackupVaultReaderRoleAssignment bool

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

var vaultName = '${resourcePrefix}-${abbreviations.backupVaults}'

module backupVaultModule '../../../common/components/data-protection/backupVault.bicep' = {
  name: '${vaultName}Deploy'
  params: {
    name: vaultName
    location: location
    vaultStorageRedundancy: 'GeoRedundant'
    tagValues: tagValues
  }
}

module resourceGroupReaderRoleAssignmentModule '../../../common/components/resource-group/roleAssignment.bicep' = if (deployBackupVaultReaderRoleAssignment) {
  name: '${vaultName}ResourceGroupRoleAssignmentDeploy'
  params: {
    principalIds: [backupVaultModule.outputs.principalId]
    role: 'Reader'
  }
}

output vaultName string = vaultName
