import { AzureSqlDatabaseConfig, SqlDatabaseDefinition } from 'types.bicep'

@description('Name of the SQL Server that this database belongs to.')
param sqlServerName string

@description('Specifies the location for this database.')
param location string

@description('Name of the database, e.g. "statistics" or "content".')
param resourceName string

@description('Configuration for the database.')
param config AzureSqlDatabaseConfig

@description('Settings specific to whether this is a primary database or a geo-replica.')
param extendedConfig SqlDatabaseDefinition.extendedConfig

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

var primaryDatabaseProperties = {
  requestedBackupStorageRedundancy: 'Geo'
}

var geoReplicaDatabaseProperties = {
  createMode: 'OnlineSecondary'
  sourceDatabaseId: extendedConfig.?geoReplicaSourceDatabaseId
  secondaryType: 'Geo'
}

// Server and database names are combined to keep alert and deployment names unique across SQL Servers.
var alertsResourceName = '${sqlServerName}-${resourceName}'

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
    // Workaround for Bicep validation on minCapacity which doesn't accept float values.
    minCapacity: config.?minCapacity != null ? json(config.minCapacity!) : null
    ...(extendedConfig.type == 'georeplica' ? geoReplicaDatabaseProperties : primaryDatabaseProperties)
  }
  tags: databaseTagValues
}

// Backup, auditing and encryption settings are inherited from the primary database by geo-replicas.
module primaryDatabaseSettingsModule 'primary-database-settings.bicep' = if (extendedConfig.type != 'georeplica') {
  name: '${sqlServerName}-${resourceName}PrimarySettingsDeploy'
  params: {
    sqlServerName: sqlServerName
    databaseName: database.name
    longTermWeeklyRetention: extendedConfig.longTermWeeklyRetention
    longTermMonthlyRetention: extendedConfig.longTermMonthlyRetention
    longTermYearlyRetention: extendedConfig.longTermYearlyRetention
    longTermRetentionWeekOfYear: extendedConfig.longTermRetentionWeekOfYear
  }
}

// Diagnostics are applied to both primaries and replicas.
resource databaseDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'serverAuditToLogAnalytics'
  scope: database
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: databaseDiagnosticsLogsAndMetrics.logs
    metrics: databaseDiagnosticsLogsAndMetrics.metrics
  }
  // Added to prevent diagnosticSettings from rolling out in parallel with primary
  // database settings. In the case of a replica, this can execute immediately.
  dependsOn: [
    primaryDatabaseSettingsModule
  ]
}

module databaseAlertsModule 'database-alerts.bicep' = {
  name: '${alertsResourceName}AlertsDeploy'
  params: {
    databaseAlertsPrefix: alertsResourceName
    databaseId: database.id
    alertsGroupName: alertsGroupName
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

output databaseName string = database.name
output databaseId string = database.id
