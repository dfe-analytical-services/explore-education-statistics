@description('Name of the SQL Server that these diagnostic and auditing settings belong to.')
param sqlServerName string

@description('Email addresses to notify for security alerts and vulnerability assessment scans.')
param teamEmailAddresses string[]

@description('Number of days to retain database audit logs for in blob storage.')
param databaseAuditBlobRetentionDays int = 365

@description('Name of the storage account that database audit logs and vulnerability assessment scans are written to.')
param loggingStorageAccountName string

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

resource sqlServer 'Microsoft.Sql/servers@2025-01-01' existing = {
  name: sqlServerName
}

resource masterDb 'Microsoft.Sql/servers/databases@2025-01-01' existing = {
  parent: sqlServer
  name: 'master'
}

resource loggingStorageAccount 'Microsoft.Storage/storageAccounts@2026-04-01' existing = {
  name: loggingStorageAccountName
}

var loggingStorageEndpoint = loggingStorageAccount.properties.primaryEndpoints.blob
var loggingStorageAccessKey = loggingStorageAccount.listKeys().keys[0].value

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
