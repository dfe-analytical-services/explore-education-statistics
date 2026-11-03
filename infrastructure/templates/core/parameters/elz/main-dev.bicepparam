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

// The release pipeline grants Admin's and the Publisher's user-assigned identities their roles instead.
param deploySystemAssignedIdentityRoleAssignments = false

// The SQL Servers write auditing and vulnerability assessment output to the logging storage account with their
// system-assigned identities.
param sqlServerIdentityBasedAuditStorage = true

// The apps connect to the core and public storage accounts with their managed identities, so their connection strings
// aren't stored.
param storeStorageAccountConnectionStrings = false

// Everything connects to the core, public and logging storage accounts with managed identities, so they don't accept
// account keys.
param storageSharedKeyAccessEnabled = false

// New environments take the standard names for role assignments and private endpoint connections.
param keepLegacyNames = false
