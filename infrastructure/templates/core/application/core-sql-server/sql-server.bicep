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

resource entraIdAdministrator 'Microsoft.Sql/servers/administrators@2025-01-01' = {
  parent: sqlServer
  name: 'activeDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: sqlAzureAdministratorLogin
    sid: sqlAzureAdministratorSid
    tenantId: az.subscription().tenantId
  }
  dependsOn: [
    contentDbModule
  ]
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

module networkingModule 'networking.bicep' = {
  name: 'coreSqlServerNetworkingDeploy'
  params: {
    sqlServerName: sqlServer.name
    location: location
    sqlServerPublicNetworkAccess: sqlServerPublicNetworkAccess
    firewallRules: firewallRules
    subnets: subnets
    tagValues: tagValues
  }
}

module contentDbModule 'database.bicep' = {
  name: 'contentDbDeploy'
  params: {
    sqlServerName: sqlServer.name
    location: location
    resourceName: 'content'
    config: contentDbConfig
    longTermMonthlyRetention: 'P12M'
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    alertsGroupName: alertsGroupName
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
  dependsOn: [
    masterDbDiagnostics
  ]
}

module statisticsDbModule 'database.bicep' = {
  name: 'statisticsDbDeploy'
  params: {
    sqlServerName: sqlServer.name
    location: location
    resourceName: 'statistics'
    config: statisticsDbConfig
    longTermMonthlyRetention: 'P3M'
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    alertsGroupName: alertsGroupName
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
  dependsOn: [
    masterDbDiagnostics
  ]
}

output serverName string = sqlServer.name
output serverFqdn string = sqlServer.properties.fullyQualifiedDomainName
