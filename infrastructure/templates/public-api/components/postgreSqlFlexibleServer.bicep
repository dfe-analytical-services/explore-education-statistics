import {
  cpuPercentageConfig
  memoryPercentageConfig
  dynamicMaxGreaterThan
  dynamicAverageGreaterThan
  dynamicTotalGreaterThan
} from 'alerts/dynamicAlertConfig.bicep'

import { staticAverageLessThanHundred, capacity, staticTotalGreaterThanZero } from 'alerts/staticAlertConfig.bicep'

import { IpRange, PrincipalNameAndId, PostgreSqlFlexibleServerConfig } from '../types.bicep'

@description('Specifies the location for all resources.')
param location string

@description('Server Name for Azure Database for PostgreSQL')
param databaseServerName string = ''

@description('Database administrator login name')
@minLength(0)
param adminName string

@description('Database administrator password')
@minLength(8)
@secure()
param adminPassword string

@description('Server configuration.')
param serverConfig PostgreSqlFlexibleServerConfig

@description('An array of database names to create within this server')
param databaseNames string[]

@description('An array of firewall rules containing IP address ranges that can access this server')
param firewallRules IpRange[] = []

@description('An array of Entra ID admin principal names for this resource')
param entraIdAdminPrincipals PrincipalNameAndId[] = []

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  availability: bool
  queryTime: bool
  transactionTime: bool
  clientConenctionsWaiting: bool
  cpuPercentage: bool
  diskBandwidth: bool
  diskIops: bool
  memoryPercentage: bool
  capacity: bool
  failedConnections: bool
  deadlocks: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

@description('Create mode for the PostgreSQL Flexible Server resource')
@allowed([
  'Create'
  'Default'
  'GeoRestore'
  'PointInTimeRestore'
  'Replica'
  'ReviveDropped'
  'Update'
])
param createMode string = 'Default'

@description('The id of the subnet which will be used to install the private endpoint for allowing secure connection to the database server over the VNet')
param privateEndpointSubnetId string

resource postgreSQLDatabase 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: databaseServerName
  location: location
  sku: {
    tier: serverConfig.sku.pricingTier
    name: serverConfig.sku.compute
  }
  properties: {
    createMode: createMode
    version: serverConfig.server.postgreSqlVersion
    administratorLogin: adminName
    administratorLoginPassword: adminPassword
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Disabled'
      tenantId: tenant().tenantId
    }
    storage: {
      storageSizeGB: serverConfig.storage.storageSizeGB
      autoGrow: serverConfig.storage.autoGrow ? 'Enabled' : 'Disabled'
    }
    backup: {
      backupRetentionDays: serverConfig.backups.retentionDays
      geoRedundantBackup: serverConfig.backups.geoRedundantBackup ? 'Enabled' : 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }

  resource database 'databases' = [
    for name in databaseNames: {
      name: name
    }
  ]

  tags: tagValues
}

resource settings 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2022-12-01' = [for setting in serverConfig.settings: {
  parent: postgreSQLDatabase
  name: setting.name
  properties: {
    value: setting.value
    source: 'user-override'
  }
}]

@batchSize(1)
resource firewallRuleAssignments 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2022-12-01' = [
  for rule in firewallRules: {
    name: rule.name
    parent: postgreSQLDatabase
    properties: {
      startIpAddress: parseCidr(rule.cidr).firstUsable
      endIpAddress: parseCidr(rule.cidr).lastUsable
    }
  }
]

module privateEndpointModule 'privateEndpoint.bicep' = {
  name: 'postgresPrivateEndpointDeploy'
  params: {
    serviceId: postgreSQLDatabase.id
    serviceName: postgreSQLDatabase.name
    serviceType: 'postgres'
    subnetId: privateEndpointSubnetId
    location: location
    tagValues: tagValues
  }
}

@batchSize(1)
resource adminRoleAssignments 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2022-12-01' = [
  for adminPrincipal in entraIdAdminPrincipals: {
    name: adminPrincipal.objectId
    parent: postgreSQLDatabase
    properties: {
      tenantId: tenant().tenantId
      principalName: adminPrincipal.principalName
      principalType: 'USER'
    }
    dependsOn: [
      firewallRuleAssignments
    ]
  }
]

module databaseAliveAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.availability) {
  name: '${databaseServerName}DbAliveAlertModule'
  params: {
    resourceName: databaseServerName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'is_db_alive'
    }
    config: {
      ...staticAverageLessThanHundred
      nameSuffix: 'database-alive'
      threshold: '1'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module queryTimeAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.queryTime) {
  name: '${databaseServerName}QueryTimeAlertModule'
  params: {
    resourceName: databaseServerName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'longest_query_time_sec'
    }
    config: {
      ...dynamicMaxGreaterThan
      nameSuffix: 'max-query-time'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module transactionTimeAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.transactionTime) {
  name: '${databaseServerName}TransactionTimeAlertModule'
  params: {
    resourceName: databaseServerName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'longest_transaction_time_sec'
    }
    config: {
      ...dynamicMaxGreaterThan
      nameSuffix: 'max-transaction-time'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module clientConnectionsWaitingAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.clientConenctionsWaiting) {
  name: '${databaseServerName}ClientConnectionsAlertModule'
  params: {
    resourceName: databaseServerName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'client_connections_waiting'
    }
    config: {
      ...dynamicMaxGreaterThan
      nameSuffix: 'client-connections'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module cpuPercentageAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.cpuPercentage) {
  name: '${databaseServerName}CpuPercentageDeploy'
  params: {
    resourceName: databaseServerName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'cpu_percent'
    }
    config: cpuPercentageConfig
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module diskBandwidthAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.diskBandwidth) {
  name: '${databaseServerName}DiskBandwidthAlertModule'
  params: {
    resourceName: databaseServerName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'disk_bandwidth_consumed_percentage'
    }
    config: {
      ...dynamicAverageGreaterThan
      nameSuffix: 'disk-bandwidth'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module diskIopsAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.diskIops) {
  name: '${databaseServerName}DiskIopsAlertModule'
  params: {
    resourceName: databaseServerName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'disk_iops_consumed_percentage'
    }
    config: {
      ...dynamicAverageGreaterThan
      nameSuffix: 'disk-iops'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module memoryPercentageAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.memoryPercentage) {
  name: '${databaseServerName}MemoryPercentageDeploy'
  params: {
    resourceName: databaseServerName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'memory_percent'
    }
    config: memoryPercentageConfig
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

var capacityAlertThresholds = [{
  threshold: 85
  severity: 'Informational'
  description: 'PostgreSQL Flexible Server is at 85% of its storage capacity.'
}
{
  threshold: 90
  severity: 'Warning'
  description: 'PostgreSQL Flexible Server is at 90% of its storage capacity.'
}
{
  threshold: 95
  severity: 'Critical'
  description: 'PostgreSQL Flexible Server is at 95% of its storage capacity.'
}]

module capacityAlerts 'alerts/staticMetricAlert.bicep' = [for capacityThreshold in capacityAlertThresholds: if (alerts != null && alerts!.capacity) {
  name: '${databaseServerName}Capacity${capacityThreshold.threshold}Deploy'
  params: {
    resourceName: databaseServerName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'storage_percent'
    }
    config: {
      ...capacity
      nameSuffix: '${capacityThreshold.threshold}-capacity'
      severity: capacityThreshold.severity
    }
    fullDescription: capacityThreshold.description
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}]

module failedConnectionsAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.failedConnections) {
  name: '${databaseServerName}FailedConnectionsDeploy'
  params: {
    resourceName: databaseServerName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'connections_failed'
    }
    config: {
      ...dynamicTotalGreaterThan
      nameSuffix: 'failed-connections'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module deadlocksAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.deadlocks) {
  name: '${databaseServerName}DeadlocksDeploy'
  params: {
    resourceName: databaseServerName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'deadlocks'
      dimensions: [{
        name: 'DatabaseName'
        values: databaseNames
      }]
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'deadlocks'
      windowSize: 'PT5M'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

@description('The fully qualified Azure resource ID of the Database Server.')
output databaseRef string = resourceId('Microsoft.DBforPostgreSQL/flexibleServers', databaseServerName)

@description('A template connection string to be used with managed identities and access tokens.')
output managedIdentityConnectionStringTemplate string = 'Server=${postgreSQLDatabase.name}.postgres.database.azure.com;Database=[database_name];Port=5432;User Id=[managed_identity_name]'
