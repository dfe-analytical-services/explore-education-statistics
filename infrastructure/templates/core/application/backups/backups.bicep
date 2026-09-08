@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Whether or not to create role assignments necessary for performing certain backup actions.')
param deployBackupVaultReaderRoleAssignment bool

@description('Whether or not to create or update Recovery Services Vault and policies.')
param deployRecoveryServicesVault bool

@description('Should the Recovery Services Vault be set to immutable?')
param recoveryServicesVaultImmutable bool

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

module backupVaultModule 'backup-vault.bicep' = {
  name: 'backupVaultModuleDeploy'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    deployBackupVaultReaderRoleAssignment: deployBackupVaultReaderRoleAssignment
    tagValues: tagValues
  }
}

module backupBlobsPolicyModule 'backup-vault-blobs-policy.bicep' = {
  name: 'backupVaultBlobsPolicyModuleDeploy'
  params: {
    vaultName: backupVaultModule.outputs.vaultName
    resourcePrefix: resourcePrefix
  }
}

module backupPsqlFlexibleServerPolicyModule 'backup-vault-psql-flexibleserver-policy.bicep' = {
  name: 'backupVaultPsqlFlexibleServerPolicyModuleDeploy'
  params: {
    vaultName: backupVaultModule.outputs.vaultName
    resourcePrefix: resourcePrefix
  }
}

module recoveryServicesVaultModule 'recovery-services-vault.bicep' = if (deployRecoveryServicesVault) {
  name: 'recoveryServicesVaultApplicationModuleDeploy'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    immutable: recoveryServicesVaultImmutable
    tagValues: tagValues
  }
}
