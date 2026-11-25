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

// Allow Backup Vault PostgreSQL backups to be restored in the paired region, which the current subscription's
// param file turns off.
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
