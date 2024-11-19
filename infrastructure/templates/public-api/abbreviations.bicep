// Originally sourced from https://github.com/Azure-Samples/todo-csharp-sql/blob/main/infra/abbreviations.json.
@export()
var abbreviations = {
  appContainerApps: 'ca'
  appManagedEnvironments: 'cae'
  // TODO - remove the "-flexibleserver" suffix and change the suffix of our PSQL instance to "-01"
  dBforPostgreSQLServers: 'psql-flexibleserver'
  // 'ai' is non-standard - it should be 'appi'
  insightsComponents: 'ai'
  managedIdentityUserAssignedIdentities: 'id'
  networkApplicationGateways: 'agw'
  operationalInsightsWorkspaces: 'log'
  staticWebApps: 'stapp'
  // 'sa' is non-standard - it should be 'st'
  storageStorageAccounts: 'sa'
  // 'fa' is non-standard - it shoule be 'func'
  webSitesFunctions: 'fa'
  // 'asp' is non-standard - it should be 'plan'
  webServerFarms: 'asp'
}
