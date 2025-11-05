@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param resourcePrefix string = resourceGroup().location

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Whether or not to create role assignments necessary for performing certain backup actions.')
param deployBackupVaultReaderRoleAssignment bool = false

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

module backupVaultModule 'backupVault.bicep' = {
  name: 'backupVaultModuleDeploy'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    deployBackupVaultReaderRoleAssignment: deployBackupVaultReaderRoleAssignment
    tagValues: tagValues
  }
}

module backupBlobsPolicyModule 'backupVaultBlobsPolicy.bicep' = {
  name: 'backupVaultBlobsPolicyModuleDeploy'
  params: {
    vaultName: backupVaultModule.outputs.vaultName
    resourcePrefix: resourcePrefix
  }
}

module backupPsqlFlexibleServerPolicyModule 'backupVaultPsqlFlexibleServerPolicy.bicep' = {
  name: 'backupVaultPsqlFlexibleServerPolicyModuleDeploy'
  params: {
    vaultName: backupVaultModule.outputs.vaultName
    resourcePrefix: resourcePrefix
  }
}
