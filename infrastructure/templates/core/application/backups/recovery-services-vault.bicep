@description('Name of the Recovery Services Vault.')
param vaultName string

@description('The location to create resources in.')
param location string

@description('Whether manual deletion of backups is allowed.')
param immutable bool

@description('Tags for the resources')
param tagValues object

module recoveryVaultModule '../../../common/components/recovery-services-vault/recovery-services-vault.bicep' = {
  name: 'recoveryVaultDeploy'
  params: {
    vaultName: vaultName
    location: location
    redundancy: 'GeoRedundant'
    immutable: immutable
    softDelete: {
      retentionDays: 30
      state: 'AlwaysON'
    }
    crossSubscriptionRestoreEnabled: false
    crossRegionRestoreEnabled: true
    alertsEnabled: true
    tagValues: tagValues
  }
}

module fileShareBackupPolicyModule '../../../common/components/recovery-services-vault/file-share-backup-policy.bicep' = {
  name: 'fileShareBackupPolicyDeploy'
  params: {
    policyName: 'DailyPolicy'
    vaultName: vaultName
    location: location
    dailyBackupTimeUtc: '00:00'
    daysToRetainDailyBackups: 30
    weeklySnapshotRetention: {
      targetBackupDay: 'Monday'
      weeksToRetain: 8
    }
    monthlySnapshotRetention: {
      targetBackupDay: 'Monday'
      targetBackupWeek: 'First'
      monthsToRetain: 12
    }
    tagValues: tagValues
  }
  dependsOn: [
    recoveryVaultModule
  ]
}

output recoveryServicesVaultName string = vaultName
