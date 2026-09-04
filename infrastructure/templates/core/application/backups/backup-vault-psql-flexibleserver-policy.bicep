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
    vaultTierWeeklyBackupScheduleTime: '07:00'
    vaultTierDefaultRetentionInYears: 1
    vaultTierWeeklyRetentionInWeeks: 12
    vaultTierMonthlyRetentionInMonths: 12
    vaultTierYearlyRetentionInYears: 2
  }
}
