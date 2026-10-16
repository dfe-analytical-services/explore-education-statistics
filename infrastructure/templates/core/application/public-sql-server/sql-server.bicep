import { VNetSubnets } from '../virtual-network/types.bicep'
import { AzureSqlDatabaseConfig } from '../../../common/components/azure-sql/types.bicep'
import { IpRange } from '../../../common/types.bicep'

@description('Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string

@description('Specifies the location for all resources.')
param location string = resourceGroup().location

@description('The admin user of the SQL Server.')
param sqlAdministratorLogin string

@description('The Key Vault instance that holds the SQL Server admin password.')
param keyVaultName string

@description('The login name of the Entra ID admin for the SQL Server.')
param sqlAzureAdministratorLogin string

@description('The object id of the Entra ID admin for the SQL Server.')
param sqlAzureAdministratorSid string

@description('Whether or not public access is enabled for the SQL Server. Firewall and VNet rules only apply when this is Enabled.')
param sqlServerPublicNetworkAccess 'Enabled' | 'Disabled' = 'Enabled'

@description('Subnets from the VNet.')
param subnets VNetSubnets

@description('Firewall rules for the SQL Server, applied when public network access is enabled.')
param firewallRules IpRange[] = []

@description('Email addresses to notify for security alerts and vulnerability assessment scans.')
param teamEmailAddresses string[]

@description('Number of days to retain database audit logs for in blob storage.')
param databaseAuditBlobRetentionDays int = 365

@description('Name of the storage account that database audit logs and vulnerability assessment scans are written to.')
param loggingStorageAccountName string

@description('Configuration for the Statistics database geo-replica.')
param statisticsDbConfig AzureSqlDatabaseConfig

@description('Resource id of the primary Statistics database that the Statistics database geo-replica is created from.')
param statisticsPrimaryDatabaseId string

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('Name of the Action Group that receives alerts.')
param alertsGroupName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Tags for the resources')
param tagValues object

var publicSqlServerName = '${subscription}-sqlsvr-ees-02'

resource keyVault 'Microsoft.KeyVault/vaults@2026-02-01' existing = {
  name: keyVaultName
}

// Rule names are retained from the original ARM template.
var allowedSubnets = [
  { name: 'content', id: subnets.content.id }
  { name: 'data', id: subnets.data.id }
  { name: 'publisher', id: subnets.publisher.id }
]

module sqlServerModule '../../../common/components/azure-sql/sql-server.bicep' = {
  name: 'publicSqlServerDeploy'
  params: {
    serverName: publicSqlServerName
    location: location
    sqlAdministratorLogin: sqlAdministratorLogin
    sqlAdministratorLoginPassword: keyVault.getSecret('ees-sql-admin-password')
    sqlAzureAdministratorLogin: sqlAzureAdministratorLogin
    sqlAzureAdministratorSid: sqlAzureAdministratorSid
    sqlServerPublicNetworkAccess: sqlServerPublicNetworkAccess
    allowedSubnets: allowedSubnets
    privateEndpointSubnetId: subnets.sqlServerPrivateEndpoints.id
    // TODO EES-7502 - use standardised name for private endpoint connections.
    privateLinkServiceConnectionNameOverride: '${publicSqlServerName}-pep-conn'
    firewallRules: firewallRules
    teamEmailAddresses: teamEmailAddresses
    databaseAuditBlobRetentionDays: databaseAuditBlobRetentionDays
    loggingStorageAccountName: loggingStorageAccountName
    databases: [
      {
        name: 'statistics'
        config: statisticsDbConfig
        extendedConfig: {
          type: 'georeplica'
          geoReplicaSourceDatabaseId: statisticsPrimaryDatabaseId
        }
      }
    ]
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    alertsGroupName: alertsGroupName
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}
