import { getFullBackupVaultDataSourceType } from 'functions.bicep'

@description('Name of the backup policy.')
param policyName string

@description('Name of the backup vault that this policy belongs to.')
param vaultName string

@description('Vault tier default backup retention duration in years.')
@minValue(1)
@maxValue(10)
param vaultTierDefaultRetentionInYears int

@description('Vault tier weekly backup retention duration in weeks.')
@minValue(4)
@maxValue(521)
param vaultTierWeeklyRetentionInWeeks int

@description('Vault tier monthly backup retention duration in months.')
@minValue(5)
@maxValue(116)
param vaultTierMonthlyRetentionInMonths int

@description('Vault tier yearly backup retention duration in years.')
@minValue(1)
@maxValue(10)
param vaultTierYearlyRetentionInYears int

@description('Vault tier weekly backup schedule time, in hh:mm format e.g. 07:00.')
param vaultTierWeeklyBackupScheduleTime string

var vaultTierDefaultRetentionDuration = 'P${vaultTierDefaultRetentionInYears}Y'
var vaultTierWeeklyRetentionDuration = 'P${vaultTierWeeklyRetentionInWeeks}W'
var vaultTierMonthlyRetentionDuration = 'P${vaultTierMonthlyRetentionInMonths}M'
var vaultTierYearlyRetentionDuration = 'P${vaultTierYearlyRetentionInYears}Y'
var repeatingTimeIntervals = 'R/2024-05-06T${vaultTierWeeklyBackupScheduleTime}:00+00:00/P1W'

resource backupPolicy 'Microsoft.DataProtection/backupVaults/backupPolicies@2025-07-01' = {
  name: '${vaultName}/${policyName}'
  properties: {
    objectType: 'BackupPolicy'
    datasourceTypes: [getFullBackupVaultDataSourceType('psqlFlexibleServer')]
    policyRules: [
      {
        name: 'Default'
        objectType: 'AzureRetentionRule'
        isDefault: true
        lifecycles: [
          {
            deleteAfter: {
              duration: vaultTierDefaultRetentionDuration
              objectType: 'AbsoluteDeleteOption'
            }
            sourceDataStore: {
              dataStoreType: 'VaultStore'
              objectType: 'DataStoreInfoBase'
            }
            targetDataStoreCopySettings: []
          }
        ]
      }
      {
        name: 'Yearly'
        objectType: 'AzureRetentionRule'
        isDefault: false
        lifecycles: [
          {
            deleteAfter: {
              duration: vaultTierYearlyRetentionDuration
              objectType: 'AbsoluteDeleteOption'
            }
            sourceDataStore: {
              dataStoreType: 'VaultStore'
              objectType: 'DataStoreInfoBase'
            }
            targetDataStoreCopySettings: []
          }
        ]
      }
      {
        name: 'Monthly'
        objectType: 'AzureRetentionRule'
        isDefault: false
        lifecycles: [
          {
            deleteAfter: {
              duration: vaultTierMonthlyRetentionDuration
              objectType: 'AbsoluteDeleteOption'
            }
            sourceDataStore: {
              dataStoreType: 'VaultStore'
              objectType: 'DataStoreInfoBase'
            }
            targetDataStoreCopySettings: []
          }
        ]
      }
      {
        name: 'Weekly'
        objectType: 'AzureRetentionRule'
        isDefault: false
        lifecycles: [
          {
            deleteAfter: {
              duration: vaultTierWeeklyRetentionDuration
              objectType: 'AbsoluteDeleteOption'
            }
            sourceDataStore: {
              dataStoreType: 'VaultStore'
              objectType: 'DataStoreInfoBase'
            }
            targetDataStoreCopySettings: []
          }
        ]
      }
      {
        name: 'BackupWeekly'
        objectType: 'AzureBackupRule'
        backupParameters: {
          backupType: 'Full'
          objectType: 'AzureBackupParams'
        }
        dataStore: {
          dataStoreType: 'VaultStore'
          objectType: 'DataStoreInfoBase'
        }
        trigger: {
          objectType: 'ScheduleBasedTriggerContext'
          schedule: {
            timeZone: 'UTC'
            repeatingTimeIntervals: [
              repeatingTimeIntervals
            ]
          }
          taggingCriteria: [
            {
              isDefault: false
              taggingPriority: 10
              tagInfo: {
                tagName: 'Yearly'
              }
              criteria: [
                {
                  absoluteCriteria: [
                    'FirstOfYear'
                  ]
                  objectType: 'ScheduleBasedBackupCriteria'
                }
              ]
            }
            {
              isDefault: false
              taggingPriority: 15
              tagInfo: {
                tagName: 'Monthly'
              }
              criteria: [
                {
                  absoluteCriteria: [
                    'FirstOfMonth'
                  ]
                  objectType: 'ScheduleBasedBackupCriteria'
                }
              ]
            }
            {
              isDefault: false
              taggingPriority: 20
              tagInfo: {
                tagName: 'Weekly'
              }
              criteria: [
                {
                  absoluteCriteria: [
                    'FirstOfWeek'
                  ]
                  objectType: 'ScheduleBasedBackupCriteria'
                }
              ]
            }
            {
              isDefault: true
              taggingPriority: 99
              tagInfo: {
                tagName: 'Default'
              }
            }
          ]
        }
      }
    ]
  }
}
