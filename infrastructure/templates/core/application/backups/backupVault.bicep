import { abbreviations } from '../../../common/abbreviations.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

// Our deploy SPN currently does not have permission to assign this role. 
var deployBackupVaultReaderRoleAssignment = false

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
