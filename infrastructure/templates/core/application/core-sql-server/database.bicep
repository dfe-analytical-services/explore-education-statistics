import { AzureSqlDatabaseConfig } from 'types.bicep'

@description('Name of the SQL Server that this database belongs to.')
param sqlServerName string

@description('Specifies the location for this database.')
param location string

@description('Name of the database, e.g. "statistics" or "content".')
param resourceName string

@description('Configuration for the database.')
param config AzureSqlDatabaseConfig

@description('Monthly long term retention, e.g. "P12M" or "P3M".')
param longTermMonthlyRetention string

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('Name of the Action Group that receives alerts.')
param alertsGroupName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Tags for the resources')
param tagValues object

var databaseTagValues = union(tagValues, {
  ServiceType: 'SQL Database'
})

var databaseDiagnosticsLogsAndMetrics = {
  logs: [
    { category: 'SQLSecurityAuditEvents', enabled: true }
    { category: 'Errors', enabled: true }
    { category: 'Timeouts', enabled: true }
    { category: 'Blocks', enabled: true }
    { category: 'Deadlocks', enabled: true }
    { category: 'DatabaseWaitStatistics', enabled: true }
  ]
  metrics: [
    { category: 'Basic', enabled: true }
    { category: 'InstanceAndAppAdvanced', enabled: true }
    { category: 'WorkloadManagement', enabled: true }
  ]
}

resource sqlServer 'Microsoft.Sql/servers@2025-01-01' existing = {
  name: sqlServerName
}

resource database 'Microsoft.Sql/servers/databases@2025-01-01' = {
  parent: sqlServer
  name: resourceName
  location: location
  sku: config.sku
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    licenseType: config.licenseType
    maxSizeBytes: config.maxSizeBytes
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    autoPauseDelay: -1
    requestedBackupStorageRedundancy: 'GRS'
    minCapacity: json(string(config.?minCapacity))
  }
  tags: databaseTagValues
}

resource databaseAuditingSettings 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2025-01-01' = {
  parent: database
  name: 'default'
  properties: {
    state: 'Disabled'
  }
}

resource databaseDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'serverAuditToLogAnalytics'
  scope: database
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: databaseDiagnosticsLogsAndMetrics.logs
    metrics: databaseDiagnosticsLogsAndMetrics.metrics
  }
  dependsOn: [
    databaseAuditingSettings
  ]
}

resource databaseLongTermRetentionPolicy 'Microsoft.Sql/servers/databases/backupLongTermRetentionPolicies@2025-01-01' = {
  parent: database
  name: 'default'
  properties: {
    weeklyRetention: 'P4W'
    monthlyRetention: longTermMonthlyRetention
    yearlyRetention: 'P1Y'
    weekOfYear: 1
  }
}

resource databaseTransparentDataEncryption 'Microsoft.Sql/servers/databases/transparentDataEncryption@2025-01-01' = {
  parent: database
  name: 'current'
  properties: {
    state: 'Enabled'
  }
}

module databaseAlertsModule 'database-alerts.bicep' = {
  name: '${resourceName}DbAlertsDeploy'
  params: {
    resourceName: resourceName
    databaseId: database.id
    alertsGroupName: alertsGroupName
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

output databaseName string = database.name
output databaseId string = database.id
