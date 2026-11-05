// ELZ version of ../main-dev.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../main.bicep'
extends '../main-dev.bicepparam'

param elz = true

// Zone-redundant storage.
param importerStorageAccountSku = 'Standard_ZRS'
param notifierStorageAccountSku = 'Standard_ZRS'
param publisherStorageAccountSku = 'Standard_ZRS'

// Host every App Service and Function App on Linux.
param appOperatingSystem = 'Linux'

// Deploy a user-assigned managed identity for each of this pipeline's apps.
param deployAppIdentities = true
