import { VNetSubnets } from '../virtual-network/types.bicep'
import { AzureSqlDatabaseConfig } from 'types.bicep'

@description('Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string

@description('Specifies the location for all resources.')
param location string = resourceGroup().location

@description('The admin user of the SQL Server.')
param sqlAdministratorLogin string

@secure()
@description('The password of the admin user of the SQL Server.')
param sqlAdministratorLoginPassword string

@description('The login name of the Entra ID admin for the SQL Server.')
param sqlAzureAdministratorLogin string

@description('The object id of the Entra ID admin for the SQL Server.')
param sqlAzureAdministratorSid string

@description('Minimum TLS version supported.')
param minTlsVersion string = '1.2'

@description('Whether or not public access is enabled for the SQL Server. Firewall and VNet rules only apply when this is Enabled.')
param sqlServerPublicNetworkAccess 'Enabled' | 'Disabled' = 'Enabled'

@description('Subnets from the VNet.')
param subnets VNetSubnets

@description('Firewall rules for the SQL Server, applied when public network access is enabled.')
param firewallRules {
  name: string
  startIpAddress: string
  endIpAddress: string
}[] = []

@description('Email addresses to notify for security alerts and vulnerability assessment scans.')
param teamEmailAddresses string[]

@description('Number of days to retain database audit logs for in blob storage.')
param databaseAuditBlobRetentionDays int = 365

@description('Name of the storage account that database audit logs and vulnerability assessment scans are written to.')
param loggingStorageAccountName string

@description('Configuration for the Content database.')
param contentDbConfig AzureSqlDatabaseConfig

@description('Configuration for the Statistics database.')
param statisticsDbConfig AzureSqlDatabaseConfig

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('Name of the Action Group that receives alerts.')
param alertsGroupName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Tags for the resources')
param tagValues object

var coreSqlServerName = '${subscription}-sqlsvr-ees-01'

var databaseTagValues = union(tagValues, {
  ServiceType: 'SQL Database'
})

resource loggingStorageAccount 'Microsoft.Storage/storageAccounts@2026-04-01' existing = {
  name: loggingStorageAccountName
}

var loggingStorageEndpoint = loggingStorageAccount.properties.primaryEndpoints.blob
var loggingStorageAccessKey = loggingStorageAccount.listKeys().keys[0].value

resource sqlServer 'Microsoft.Sql/servers@2025-01-01' = {
  name: coreSqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorLoginPassword
    version: '12.0'
    minimalTlsVersion: minTlsVersion
    publicNetworkAccess: sqlServerPublicNetworkAccess
  }
  tags: union(tagValues, {
    ServiceType: 'SQL Server'
  })
}

resource statisticsDb 'Microsoft.Sql/servers/databases@2025-01-01' = {
  parent: sqlServer
  name: 'statistics'
  location: location
  sku: statisticsDbConfig.sku
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    licenseType: statisticsDbConfig.licenseType
    maxSizeBytes: statisticsDbConfig.maxSizeBytes
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    autoPauseDelay: -1
    requestedBackupStorageRedundancy: 'GRS'
    minCapacity: json(string(statisticsDbConfig.?minCapacity))
  }
  tags: databaseTagValues
}

resource contentDb 'Microsoft.Sql/servers/databases@2025-01-01' = {
  parent: sqlServer
  name: 'content'
  location: location
  sku: contentDbConfig.sku
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    licenseType: contentDbConfig.licenseType
    maxSizeBytes: contentDbConfig.maxSizeBytes
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    autoPauseDelay: -1
    requestedBackupStorageRedundancy: 'GRS'
    minCapacity: json(string(contentDbConfig.?minCapacity))
  }
  tags: databaseTagValues
}

resource masterDb 'Microsoft.Sql/servers/databases@2025-01-01' existing = {
  parent: sqlServer
  name: 'master'
}

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

resource statisticsDbAuditingSettings 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2025-01-01' = {
  parent: statisticsDb
  name: 'default'
  properties: {
    state: 'Disabled'
  }
}

resource statisticsDbDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'serverAuditToLogAnalytics'
  scope: statisticsDb
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: databaseDiagnosticsLogsAndMetrics.logs
    metrics: databaseDiagnosticsLogsAndMetrics.metrics
  }
  dependsOn: [
    statisticsDbAuditingSettings
  ]
}

resource contentDbAuditingSettings 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2025-01-01' = {
  parent: contentDb
  name: 'default'
  properties: {
    state: 'Disabled'
  }
}

resource contentDbDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'serverAuditToLogAnalytics'
  scope: contentDb
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: databaseDiagnosticsLogsAndMetrics.logs
    metrics: databaseDiagnosticsLogsAndMetrics.metrics
  }
  dependsOn: [
    contentDbAuditingSettings
  ]
}

resource sqlServerAadAdmin 'Microsoft.Sql/servers/administrators@2025-01-01' = {
  parent: sqlServer
  name: 'activeDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: sqlAzureAdministratorLogin
    sid: sqlAzureAdministratorSid
    tenantId: az.subscription().tenantId
  }
  dependsOn: [
    contentDb
  ]
}

resource contentDbLongTermRetentionPolicy 'Microsoft.Sql/servers/databases/backupLongTermRetentionPolicies@2025-01-01' = {
  parent: contentDb
  name: 'default'
  properties: {
    weeklyRetention: 'P4W'
    monthlyRetention: 'P12M'
    yearlyRetention: 'P1Y'
    weekOfYear: 1
  }
}

resource statisticsDbLongTermRetentionPolicy 'Microsoft.Sql/servers/databases/backupLongTermRetentionPolicies@2025-01-01' = {
  parent: statisticsDb
  name: 'default'
  properties: {
    weeklyRetention: 'P4W'
    monthlyRetention: 'P3M'
    yearlyRetention: 'P1Y'
    weekOfYear: 1
  }
}

resource securityAlertPolicy 'Microsoft.Sql/servers/securityAlertPolicies@2025-01-01' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    state: 'Enabled'
    disabledAlerts: []
    emailAddresses: teamEmailAddresses
    emailAccountAdmins: true
  }
}

resource vulnerabilityAssessment 'Microsoft.Sql/servers/vulnerabilityAssessments@2025-01-01' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    storageContainerPath: '${loggingStorageEndpoint}vulnerability-assessment'
    storageAccountAccessKey: loggingStorageAccessKey
    recurringScans: {
      isEnabled: true
      emailSubscriptionAdmins: true
      emails: teamEmailAddresses
    }
  }
  dependsOn: [
    securityAlertPolicy
  ]
}

resource masterDbAuditingSettings 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2025-01-01' = {
  parent: masterDb
  name: 'default'
  properties: {
    auditActionsAndGroups: [
      'SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP'
      'FAILED_DATABASE_AUTHENTICATION_GROUP'
      'BATCH_COMPLETED_GROUP'
    ]
    state: 'Disabled'
  }
  dependsOn: [
    vulnerabilityAssessment
  ]
}

resource serverAuditingSettings 'Microsoft.Sql/servers/extendedAuditingSettings@2025-01-01' = {
  parent: sqlServer
  name: 'default'
  properties: {
    auditActionsAndGroups: [
      'SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP'
      'FAILED_DATABASE_AUTHENTICATION_GROUP'
      'BATCH_COMPLETED_GROUP'
    ]
    isAzureMonitorTargetEnabled: true
    isManagedIdentityInUse: false
    isStorageSecondaryKeyInUse: false
    state: 'Enabled'
    storageEndpoint: loggingStorageEndpoint
    storageAccountAccessKey: loggingStorageAccessKey
    storageAccountSubscriptionId: az.subscription().subscriptionId
    retentionDays: databaseAuditBlobRetentionDays
  }
  dependsOn: [
    masterDbAuditingSettings
  ]
}

resource devOpsAuditingSettings 'Microsoft.Sql/servers/devOpsAuditingSettings@2025-01-01' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    isAzureMonitorTargetEnabled: true
    isManagedIdentityInUse: false
    state: 'Enabled'
    storageEndpoint: loggingStorageEndpoint
    storageAccountAccessKey: loggingStorageAccessKey
    storageAccountSubscriptionId: az.subscription().subscriptionId
  }
  dependsOn: [
    serverAuditingSettings
  ]
}

resource masterDbDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'serverAuditToLogAnalytics'
  scope: masterDb
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: databaseDiagnosticsLogsAndMetrics.logs
    metrics: databaseDiagnosticsLogsAndMetrics.metrics
  }
  dependsOn: [
    devOpsAuditingSettings
  ]
}

resource statisticsDbTransparentDataEncryption 'Microsoft.Sql/servers/databases/transparentDataEncryption@2025-01-01' = {
  parent: statisticsDb
  name: 'current'
  properties: {
    state: 'Enabled'
  }
  dependsOn: [
    masterDbDiagnostics
  ]
}

resource contentDbTransparentDataEncryption 'Microsoft.Sql/servers/databases/transparentDataEncryption@2025-01-01' = {
  parent: contentDb
  name: 'current'
  properties: {
    state: 'Enabled'
  }
  dependsOn: [
    masterDbDiagnostics
  ]
}

resource firewallRuleResources 'Microsoft.Sql/servers/firewallRules@2025-01-01' = [
  for rule in firewallRules: if (sqlServerPublicNetworkAccess == 'Enabled') {
    parent: sqlServer
    name: rule.name
    properties: {
      startIpAddress: rule.startIpAddress
      endIpAddress: rule.endIpAddress
    }
  }
]

var sqlAllowedSubnets = [
  subnets.admin
  subnets.importer
  subnets.publisher
  subnets.content
  subnets.data
  subnets.notify
  subnets.publicApiDataProcessor
]

resource virtualNetworkRuleResources 'Microsoft.Sql/servers/virtualNetworkRules@2025-01-01' = [
  for allowedSubnet in sqlAllowedSubnets: if (sqlServerPublicNetworkAccess == 'Enabled') {
    parent: sqlServer
    name: allowedSubnet.name
    properties: {
      virtualNetworkSubnetId: allowedSubnet.id
      ignoreMissingVnetServiceEndpoint: false
    }
  }
]

module privateEndpointModule '../../../common/components/privateEndpoint.bicep' = {
  name: 'coreSqlServerPrivateEndpointDeploy'
  params: {
    serviceId: sqlServer.id
    serviceName: coreSqlServerName
    serviceType: 'azureSql'
    subnetId: subnets.sqlServerPrivateEndpoints.id
    location: location
    tagValues: tagValues
  }
}

module statisticsDbAlertsModule 'database-alerts.bicep' = {
  name: 'statisticsDbAlertsDeploy'
  params: {
    resourceName: 'statistics'
    databaseId: statisticsDb.id
    alertsGroupName: alertsGroupName
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module contentDbAlertsModule 'database-alerts.bicep' = {
  name: 'contentDbAlertsDeploy'
  params: {
    resourceName: 'content'
    databaseId: contentDb.id
    alertsGroupName: alertsGroupName
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

output serverName string = sqlServer.name
output serverFqdn string = sqlServer.properties.fullyQualifiedDomainName
