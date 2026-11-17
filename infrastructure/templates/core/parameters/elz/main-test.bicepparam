// ELZ version of ../main-test.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../main.bicep'
extends '../main-test.bicepparam'

param elz = true

// Zone-redundant storage.
param coreStorageAccountSku = 'Standard_ZRS'
param publicStorageAccountSku = 'Standard_ZRS'

// Copy file share backups into the geo-redundant Recovery Services Vault.
param vaultedFileShareBackups = true
