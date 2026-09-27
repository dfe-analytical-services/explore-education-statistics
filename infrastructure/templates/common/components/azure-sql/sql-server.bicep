import { SqlDatabaseDefinition, SubnetReference } from 'types.bicep'
import { IpRange } from '../../types.bicep'

@description('Name of the SQL Server.')
param serverName string

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

@description('Firewall rules for the SQL Server, applied when public network access is enabled.')
param firewallRules IpRange[] = []

@description('Subnets to allow direct VNet access from, applied when public network access is enabled.')
param allowedSubnets SubnetReference[] = []

@description('Id of the subnet to deploy a private endpoint into. Omit to skip deploying a private endpoint.')
param privateEndpointSubnetId string?

@description('Email addresses to notify for security alerts and vulnerability assessment scans.')
param teamEmailAddresses string[]

@description('Number of days to retain database audit logs for in blob storage.')
param databaseAuditBlobRetentionDays int = 365

@description('Name of the storage account that database audit logs and vulnerability assessment scans are written to.')
param loggingStorageAccountName string

@description('The databases to create on this SQL Server.')
param databases SqlDatabaseDefinition[]

@description('Weekly long term backup retention, applied to all databases.')
param longTermWeeklyRetention string = 'P4W'

@description('Yearly long term backup retention, applied to all databases.')
param longTermYearlyRetention string = 'P1Y'

@description('Week of the year that the yearly long term backup is taken, applied to all databases.')
param longTermRetentionWeekOfYear int = 1

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('Name of the Action Group that receives alerts.')
param alertsGroupName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Tags for the resources')
param tagValues object

resource sqlServer 'Microsoft.Sql/servers@2025-01-01' = {
  name: serverName
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
    databaseModules
  ]
}

module diagnosticsAndAuditingModule 'diagnostics-and-auditing.bicep' = {
  name: '${serverName}DiagnosticsAndAuditingDeploy'
  params: {
    sqlServerName: sqlServer.name
    teamEmailAddresses: teamEmailAddresses
    databaseAuditBlobRetentionDays: databaseAuditBlobRetentionDays
    loggingStorageAccountName: loggingStorageAccountName
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
  }
}

module networkingModule 'networking.bicep' = {
  name: '${serverName}NetworkingDeploy'
  params: {
    sqlServerName: sqlServer.name
    location: location
    sqlServerPublicNetworkAccess: sqlServerPublicNetworkAccess
    firewallRules: firewallRules
    allowedSubnets: allowedSubnets
    privateEndpointSubnetId: privateEndpointSubnetId
    tagValues: tagValues
  }
}

module databaseModules 'database.bicep' = [
  for db in databases: {
    name: '${db.name}DbDeploy'
    params: {
      sqlServerName: sqlServer.name
      location: location
      resourceName: db.name
      config: db.config
      longTermMonthlyRetention: db.longTermMonthlyRetention
      longTermWeeklyRetention: longTermWeeklyRetention
      longTermYearlyRetention: longTermYearlyRetention
      longTermRetentionWeekOfYear: longTermRetentionWeekOfYear
      logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
      alertsGroupName: alertsGroupName
      deployAlerts: deployAlerts
      tagValues: tagValues
    }
    dependsOn: [
      diagnosticsAndAuditingModule
    ]
  }
]
