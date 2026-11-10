// ELZ version of ../main-prod.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../main.bicep'
extends '../main-prod.bicepparam'

param elz = true

// Zone-redundant storage. The Notifier and Publisher hold the email subscriptions and release publishing statuses,
// so they get a readable geo copy as well.
param importerStorageAccountSku = 'Standard_ZRS'
param notifierStorageAccountSku = 'Standard_RAGZRS'
param publisherStorageAccountSku = 'Standard_RAGZRS'

// Host every App Service and Function App on Linux.
param appOperatingSystem = 'Linux'

// Spread the Admin, Content API, Data API and Public Site instances across availability zones. These plans already run
// Premium SKUs with at least 2 instances, so this costs little.
param zoneRedundantAppServicePlans = true
