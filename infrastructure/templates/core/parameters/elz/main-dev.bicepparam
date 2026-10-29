// ELZ version of ../main-dev.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../main.bicep'
extends '../main-dev.bicepparam'

param elz = true

// Zone-redundant storage.
param coreStorageAccountSku = 'Standard_ZRS'
param publicStorageAccountSku = 'Standard_ZRS'

// Copy file share backups into the geo-redundant Recovery Services Vault.
param vaultedFileShareBackups = true

// Allow Backup Vault backups to be restored in the paired region.
param backupVaultCrossRegionRestore = true
