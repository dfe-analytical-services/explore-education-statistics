import { DayOfWeek, WeekOfMonth, MonthOfYear } from '../types.bicep'

@description('The name of the backup policy')
param policyName string

@description('The name of the owning Recovery Services Vault')
param vaultName string

@description('The location for this resource.')
param location string

@description('UTC time of day at which a backup will be created, in the format HH:mm.')
param dailyBackupTimeUtc string

@description('Specifies how many days to retain a daily backup.')
param daysToRetainDailyBackups int

@description('Specifies how many weeks to retain one of the daily backups, and which backup to retain.')
param weeklySnapshotRetention {
  @description('Identifies the daily backup to retain based upon the day of the week that the backup was taken.')
  targetBackupDay: DayOfWeek

  weeksToRetain: int
}?

@description('Specifies how many months to retain one of the daily backups, and which backup to retain.')
param monthlySnapshotRetention {
  @description('''
  Identifies the daily backup to retain based upon the day of the week that the backup was taken.
  Used in conjunction with targetBackupWeek.
  ''')
  targetBackupDay: DayOfWeek

  @description('''
  Identifies the daily backup to retain based upon the week of the month that the backup was taken.
  Used in conjunction with targetBackupDay.
  ''')
  targetBackupWeek: WeekOfMonth

  monthsToRetain: int
}?

@description('Specifies how many years to retain one of the daily backups, and which backup to retain.')
param yearlySnapshotRetention {
  @description('''
  Identifies the daily backup to retain based upon the day of the week that the backup was taken.
  Used in conjunction with targetBackupWeek and targetBackupMonth.
  ''')
  targetBackupDay: DayOfWeek

  @description('''
  Identifies the daily backup to retain based upon the week of the month that the backup was taken.
  Used in conjunction with targetBackupDay and targetBackupMonth.
  ''')
  targetBackupWeek: WeekOfMonth

  @description('''
  Identifies the daily backup to retain based upon the month of the year that the backup was taken.
  Used in conjunction with targetBackupDay and targetBackupWeek.
  ''')
  targetBackupMonth: MonthOfYear

  yearsToRetain: int
}?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

resource policy 'Microsoft.RecoveryServices/vaults/backupPolicies@2024-04-30-preview' = {
  name: '${vaultName}/${policyName}'
  location: location
  properties: {
    backupManagementType: 'AzureStorage'
    workLoadType: 'AzureFileShare'
    schedulePolicy: {
      schedulePolicyType: 'SimpleSchedulePolicy'
      scheduleRunFrequency: 'Daily'
      scheduleRunTimes: [
        '2025-01-01T${dailyBackupTimeUtc}:00Z'
      ]
    }
    retentionPolicy: {
      retentionPolicyType: 'LongTermRetentionPolicy'
      dailySchedule: {
        retentionTimes: [
          '2025-01-01T${dailyBackupTimeUtc}:00Z'
        ]
        retentionDuration: {
          count: daysToRetainDailyBackups
          durationType: 'Days'
        }
      }
      weeklySchedule: weeklySnapshotRetention != null
        ? {
            daysOfTheWeek: [
              weeklySnapshotRetention!.targetBackupDay
            ]
            retentionTimes: [
              '2025-01-01T${dailyBackupTimeUtc}:00Z'
            ]
            retentionDuration: {
              count: weeklySnapshotRetention!.weeksToRetain
              durationType: 'Weeks'
            }
          }
        : null
      monthlySchedule: monthlySnapshotRetention != null
        ? {
            retentionScheduleFormatType: 'Weekly'
            retentionScheduleWeekly: {
              daysOfTheWeek: [
                monthlySnapshotRetention!.targetBackupDay
              ]
              weeksOfTheMonth: [
                monthlySnapshotRetention!.targetBackupWeek
              ]
            }
            retentionTimes: [
              '2025-01-01T${dailyBackupTimeUtc}:00Z'
            ]
            retentionDuration: {
              count: monthlySnapshotRetention!.monthsToRetain
              durationType: 'Months'
            }
          }
        : null
      yearlySchedule: yearlySnapshotRetention != null
        ? {
            retentionScheduleFormatType: 'Weekly'
            monthsOfYear: [
              yearlySnapshotRetention!.targetBackupMonth
            ]
            retentionScheduleWeekly: {
              daysOfTheWeek: [
                yearlySnapshotRetention!.targetBackupDay
              ]
              weeksOfTheMonth: [
                yearlySnapshotRetention!.targetBackupWeek
              ]
            }
            retentionTimes: [
              '2025-01-01T${dailyBackupTimeUtc}:00Z'
            ]
            retentionDuration: {
              count: yearlySnapshotRetention!.yearsToRetain
              durationType: 'Years'
            }
          }
        : null
    }
    timeZone: 'UTC'
  }
  tags: tagValues
}
