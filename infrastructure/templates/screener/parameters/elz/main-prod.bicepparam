// ELZ version of ../main-prod.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../main.bicep'
extends '../main-prod.bicepparam'

param elz = true

// Zone-redundant storage.
param storageAccountSku = 'Standard_ZRS'

// Deploy a user-assigned managed identity for each of this pipeline's apps.
param deployAppIdentities = true
