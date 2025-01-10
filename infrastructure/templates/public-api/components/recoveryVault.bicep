@description('The name of the Recovery Services Vault')
@minLength(0)
param vaultName string

@description('The location for this resource.')
param location string

@description('Whether manual deletion of backups is allowed.')
param immutable bool

@description('Soft-delete settings.')
param softDelete {
  retentionDays: int
  state: 'AlwaysON' | 'Enabled' | 'Disabled'
}

@description('Whether backups are geo-redundant, zonally-redundant or locally-redundant.')
param redundancy 'GeoRedundant' | 'ZoneRedundant' | 'LocallyRedundant'

@description('Whether cross-region restore is enabled for restoring VMs in alternative regions.')
param crossRegionRestoreEnabled bool

@description('Whether cross-subscription restore is enabled in order to restore items in a separate subscription.')
param crossSubscriptionRestoreEnabled bool

@description('Whether Azure Monitor alerts are enabled if job failures occur.')
param alertsEnabled bool

@description('SKU of the Recovery Services Vault')
param sku {
  name: string
  tier: string
} = {
  name: 'RS0'
  tier: 'Standard'
}

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

resource vault 'Microsoft.RecoveryServices/vaults@2024-04-30-preview' = {
  name: vaultName
  location: location
  sku: sku
  properties: {
    securitySettings: {
      immutabilitySettings: {
        // Note that we do not currently support "Locked" as this is completely irreversable
        // and results in a Vault that cannot be deleted. We should have protection from 
        // entire Vault deletions via delete locks on Prod regardless.
        state: immutable ? 'Unlocked' : 'Disabled'
      }
      softDeleteSettings: {
        softDeleteRetentionPeriodInDays: softDelete.retentionDays
        softDeleteState: softDelete.state
        enhancedSecurityState: 'Enabled'
      }
    }
    redundancySettings: {
      standardTierStorageRedundancy: redundancy
      crossRegionRestore: crossRegionRestoreEnabled ? 'Enabled' : 'Disabled'
    }
    monitoringSettings: {
      azureMonitorAlertSettings: {
        alertsForAllFailoverIssues: alertsEnabled ? 'Enabled' : 'Disabled'
        alertsForAllJobFailures: alertsEnabled ? 'Enabled' : 'Disabled'
        alertsForAllReplicationIssues: alertsEnabled ? 'Enabled' : 'Disabled'
      }
      classicAlertSettings: {
        alertsForCriticalOperations: 'Disabled'
        emailNotificationsForSiteRecovery: 'Disabled'
      }
    }
    restoreSettings: {
      crossSubscriptionRestoreSettings: {
        crossSubscriptionRestoreState: crossSubscriptionRestoreEnabled ? 'Enabled' : 'Disabled'
      }
    }
    publicNetworkAccess: 'Enabled'
  }
  tags: tagValues
}
