// ELZ version of ../main-test.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../main.bicep'
extends '../main-test.bicepparam'

param elz = true

// Zone-redundant storage for the Data Processor. The data file share keeps its Premium_ZRS default.
param storageAccountSku = 'Standard_ZRS'

// Admin calls this pipeline's apps with its user-assigned identity.
param adminUsesUserAssignedIdentity = true
