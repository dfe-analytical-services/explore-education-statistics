import { dataSourceTypeMap, BackupVaultPolicyDataSourceType } from 'types.bicep'

@export()
func getFullBackupVaultDataSourceType(shortType BackupVaultPolicyDataSourceType) string =>
  dataSourceTypeMap[shortType]

