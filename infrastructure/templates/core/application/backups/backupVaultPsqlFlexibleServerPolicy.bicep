import { abbreviations } from '../../../common/abbreviations.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Name of the backup vault that this policy belongs to.')
param vaultName string

var policyName = '${resourcePrefix}-psql-flexible-server-${abbreviations.backupVaultPolicies}'

module backupVaultPolicyModule '../../../common/components/data-protection/postgreSqlFlexibleServerBackupVaultPolicy.bicep' = {
  name: '${policyName}Deploy'
  params: {
    policyName: policyName
    vaultName: vaultName
    vaultTierDailyBackupScheduleTime: '07:00'
    vaultTierDefaultRetentionInDays: 30
    vaultTierWeeklyRetentionInWeeks: 52
    vaultTierMonthlyRetentionInMonths: 12
    vaultTierYearlyRetentionInYears: 1
  }
}
