// Originally sourced from https://github.com/Azure-Samples/todo-csharp-sql/blob/main/infra/abbreviations.json.
@export()
var abbreviations = {
  appContainerApps: 'ca'
  appManagedEnvironments: 'cae'
  // Added from https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-abbreviations
  backupVaults: 'bvault'
  // Added from https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-abbreviations
  backupVaultPolicies: 'bkpol'
  // TODO - remove the "-flexibleserver" suffix and change the suffix of our PSQL instance to "-01"
  dBforPostgreSQLServers: 'psql-flexibleserver'
  fileShare: 'share'
  // 'ai' is non-standard - it should be 'appi'
  insightsComponents: 'ai'
  managedIdentityUserAssignedIdentities: 'id'
  networkApplicationGateways: 'agw'
  operationalInsightsWorkspaces: 'log'
  recoveryServicesVault: 'rsv'
  staticWebApps: 'stapp'
  // 'sa' is non-standard - it should be 'st'
  storageStorageAccounts: 'sa'
  // 'fa' is non-standard - it shoule be 'func'
  webSitesFunctions: 'fa'
  // 'asp' is non-standard - it should be 'plan'
  webServerFarms: 'asp'
}
