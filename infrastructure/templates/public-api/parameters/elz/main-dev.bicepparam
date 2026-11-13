// ELZ version of ../main-dev.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../main.bicep'
extends '../main-dev.bicepparam'

param elz = true

// Zone-redundant storage for the Data Processor. The data file share keeps its Premium_ZRS default.
param storageAccountSku = 'Standard_ZRS'

// Admin calls this pipeline's apps with its user-assigned identity.
param adminUsesUserAssignedIdentity = true

// The Data Processor's host and Durable Functions connect to their storage accounts with its managed identity.
param dataProcessorIdentityBasedStorageConnections = true

// The Data Processor connects to its management storage account with its managed identity, so it doesn't accept account
// keys.
param dataProcessorManagementStorageSharedKeyAccessEnabled = false
