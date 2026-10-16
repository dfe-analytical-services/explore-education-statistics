@description('Name of the Backup Vault policy.')
param policyName string

@description('Name of the backup vault that this policy belongs to.')
param vaultName string

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
