@description('Name of the SQL Server that the database belongs to.')
param sqlServerName string

@description('Name of the primary database that these settings apply to.')
param databaseName string

@description('Weekly long term backup retention, e.g. "P4W".')
param longTermWeeklyRetention string

@description('Monthly long term backup retention, e.g. "P12M" or "P3M".')
param longTermMonthlyRetention string

@description('Yearly long term backup retention, e.g. "P1Y".')
param longTermYearlyRetention string

@description('Week of the year that the yearly long term backup is taken.')
param longTermRetentionWeekOfYear int

resource sqlServer 'Microsoft.Sql/servers@2025-01-01' existing = {
  name: sqlServerName
}

resource database 'Microsoft.Sql/servers/databases@2025-01-01' existing = {
  parent: sqlServer
  name: databaseName
}

resource databaseAuditingSettings 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2025-01-01' = {
  parent: database
  name: 'default'
  properties: {
    state: 'Disabled'
  }
}

resource databaseLongTermRetentionPolicy 'Microsoft.Sql/servers/databases/backupLongTermRetentionPolicies@2025-01-01' = {
  parent: database
  name: 'default'
  properties: {
    weeklyRetention: longTermWeeklyRetention
    monthlyRetention: longTermMonthlyRetention
    yearlyRetention: longTermYearlyRetention
    weekOfYear: longTermRetentionWeekOfYear
  }
}

resource databaseTransparentDataEncryption 'Microsoft.Sql/servers/databases/transparentDataEncryption@2025-01-01' = {
  parent: database
  name: 'current'
  properties: {
    state: 'Enabled'
  }
}
