// ELZ version of ../main-prod.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../main.bicep'
extends '../main-prod.bicepparam'

param elz = true

// Zone-redundant storage for the Data Processor. The data file share keeps its Premium_ZRS default.
param storageAccountSku = 'Standard_ZRS'

// Spread the Container App Environment across availability zones.
param containerAppEnvironmentZoneRedundant = true

// As ../main-prod.bicepparam, but with at least 2 replicas so that the Public API keeps running if a zone fails.
param publicApiContainerAppConfig = {
  cpuCores: 4
  memoryGis: 8
  minReplicas: 2
  maxReplicas: 100
  scaleAtConcurrentHttpRequests: 10
  workloadProfileName: 'Consumption'
}

// Admin calls this pipeline's apps with its user-assigned identity.
param adminUsesUserAssignedIdentity = true
