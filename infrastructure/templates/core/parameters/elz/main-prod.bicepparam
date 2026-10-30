// ELZ version of ../main-prod.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../main.bicep'
extends '../main-prod.bicepparam'

param elz = true

// Zone-redundant storage, with a readable geo copy.
param coreStorageAccountSku = 'Standard_RAGZRS'
param publicStorageAccountSku = 'Standard_RAGZRS'

// Copy file share backups into the geo-redundant Recovery Services Vault.
param vaultedFileShareBackups = true

// Allow Backup Vault backups to be restored in the paired region.
param backupVaultCrossRegionRestore = true

// The release pipeline grants Admin's and the Publisher's user-assigned identities their roles instead.
param deploySystemAssignedIdentityRoleAssignments = false
