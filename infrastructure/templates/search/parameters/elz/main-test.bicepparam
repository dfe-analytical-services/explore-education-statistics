// ELZ version of ../main-test.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../main.bicep'
extends '../main-test.bicepparam'

param elz = true

// Zone-redundant storage.
param storageAccountSku = 'Standard_ZRS'

// Deploy a user-assigned managed identity for each of this pipeline's apps.
param deployAppIdentities = true

// Search Docs connects to the search storage account with its managed identity, so its connection string isn't stored.
param identityBasedStorageConnections = true

// Everything connects to the search storage account with managed identities, so it doesn't accept account keys.
param storageSharedKeyAccessEnabled = false
