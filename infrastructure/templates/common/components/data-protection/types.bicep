@export()
type BackupVaultPolicyDataSourceType =
  | 'blobs'
  | 'psqlFlexibleServer'

@export()
var dataSourceTypeMap = {
 blobs: 'Microsoft.Storage/storageAccounts/blobServices'
 psqlFlexibleServer: 'Microsoft.DBforPostgreSQL/flexibleServers'
}
