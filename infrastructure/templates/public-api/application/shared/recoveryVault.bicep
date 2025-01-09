import { ResourceNames } from '../../types.bicep'

@description('Common resource naming variables.')
param resourceNames ResourceNames

@description('The location to create resources in.')
param location string

@description('Whether manual deletion of backups is allowed.')
param immutable bool

@description('Tags for the resources')
param tagValues object

module recoveryVaultModule '../../components/recoveryVault.bicep' = {
  name: 'recoveryVaultDeploy'
  params: {
    vaultName: resourceNames.sharedResources.recoveryVault
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

module fileShareBackupPolicyModule '../../components/recoveryVaultFileShareBackupPolicy.bicep' = {
  name: 'fileShareBackupPolicyDeploy'
  params: {
    policyName: resourceNames.sharedResources.recoveryVaultFileShareBackupPolicy
    vaultName: resourceNames.sharedResources.recoveryVault
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
