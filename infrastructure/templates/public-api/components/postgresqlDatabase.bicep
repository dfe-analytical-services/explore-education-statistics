import { IpRange, PrincipalNameAndId } from '../types.bicep'

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

@description('Azure Database for PostgreSQL sku name, typically, tier + family + cores, e.g. Standard_D4s_v3 ')
param dbSkuName string

@description('Azure Database for PostgreSQL Storage Size ')
param dbStorageSizeGB int

@description('Azure Database for PostgreSQL Autogrow setting')
param dbAutoGrowStatus string

@description('Azure Database for PostgreSQL pricing tier')
param dbSkuTier 'Burstable' | 'GeneralPurpose' | 'MemoryOptimized' = 'Burstable'

@description('PostgreSQL version')
param postgreSqlVersion '11' | '12' | '13' | '14' | '15' | '16' = '16'

@description('PostgreSQL Server backup retention days')
param backupRetentionDays int = 7

@description('Geo-Redundant Backup setting')
param geoRedundantBackup string = 'Disabled'

@description('An array of database names')
param databaseNames string[]

@description('An array of firewall rules containing IP address ranges')
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
  alertGroupName: string
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
    name: dbSkuName
    tier: dbSkuTier
  }
  properties: {
    createMode: createMode
    version: postgreSqlVersion
    administratorLogin: adminName
    administratorLoginPassword: adminPassword
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Disabled'
      tenantId: tenant().tenantId
    }
    storage: {
      storageSizeGB: dbStorageSizeGB
      autoGrow: dbAutoGrowStatus
    }
    backup: {
      backupRetentionDays: backupRetentionDays
      geoRedundantBackup: geoRedundantBackup
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }

  resource database 'databases' = [for name in databaseNames: {
      name: name
  }]

  tags: tagValues
}

@batchSize(1)
resource firewallRuleAssignments 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2022-12-01' = [for rule in firewallRules: {
  name: rule.name
  parent: postgreSQLDatabase
  properties: {
    startIpAddress: parseCidr(rule.cidr).firstUsable
    endIpAddress: parseCidr(rule.cidr).lastUsable
  }
}]

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
resource adminRoleAssignments 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2022-12-01' = [for adminPrincipal in entraIdAdminPrincipals: {
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
}]

module databaseAliveAlert 'alerts/postgreSqlFlexibleServers/databaseAlive.bicep' = if (alerts != null && alerts!.availability) {
  name: '${databaseServerName}DbAliveDeploy'
  params: {
    resourceNames: [databaseServerName]
    alertsGroupName: alerts!.alertGroupName
    tagValues: tagValues
  }
}

module queryTimeAlert 'alerts/postgreSqlFlexibleServers/queryTimeAlert.bicep' = if (alerts != null && alerts!.queryTime) {
  name: '${databaseServerName}QueryTimeDeploy'
  params: {
    resourceNames: [databaseServerName]
    alertsGroupName: alerts!.alertGroupName
    tagValues: tagValues
  }
}

module transactionTimeAlert 'alerts/postgreSqlFlexibleServers/transactionTimeAlert.bicep' = if (alerts != null && alerts!.transactionTime) {
  name: '${databaseServerName}TransactionTimeDeploy'
  params: {
    resourceNames: [databaseServerName]
    alertsGroupName: alerts!.alertGroupName
    tagValues: tagValues
  }
}

module clientConenctionsWaitingAlert 'alerts/flexibleServers/clientConnectionsWaitingAlert.bicep' = if (alerts != null && alerts!.clientConenctionsWaiting) {
  name: '${databaseServerName}ClientConnectionsDeploy'
  params: {
    resourceNames: [databaseServerName]
    alertsGroupName: alerts!.alertGroupName
    tagValues: tagValues
  }
}

module cpuPercentageAlert 'alerts/flexibleServers/cpuPercentageAlert.bicep' = if (alerts != null && alerts!.cpuPercentage) {
  name: '${databaseServerName}CpuPercentageDeploy'
  params: {
    resourceNames: [databaseServerName]
    alertsGroupName: alerts!.alertGroupName
    tagValues: tagValues
  }
}

module diskBandwidthAlert 'alerts/flexibleServers/diskBandwidthAlert.bicep' = if (alerts != null && alerts!.diskBandwidth) {
  name: '${databaseServerName}DiskBandwidthDeploy'
  params: {
    resourceNames: [databaseServerName]
    alertsGroupName: alerts!.alertGroupName
    tagValues: tagValues
  }
}

module diskIopsAlert 'alerts/flexibleServers/diskIopsAlert.bicep' = if (alerts != null && alerts!.diskIops) {
  name: '${databaseServerName}DiskIopsDeploy'
  params: {
    resourceNames: [databaseServerName]
    alertsGroupName: alerts!.alertGroupName
    tagValues: tagValues
  }
}

module memoryPercentageAlert 'alerts/flexibleServers/memoryPercentageAlert.bicep' = if (alerts != null && alerts!.memoryPercentage) {
  name: '${databaseServerName}MemoryPercentageDeploy'
  params: {
    resourceNames: [databaseServerName]
    alertsGroupName: alerts!.alertGroupName
    tagValues: tagValues
  }
}

@description('The fully qualified Azure resource ID of the Database Server.')
output databaseRef string = resourceId('Microsoft.DBforPostgreSQL/flexibleServers', databaseServerName)

@description('A template connection string to be used with managed identities and access tokens.')
output managedIdentityConnectionStringTemplate string = 'Server=${postgreSQLDatabase.name}.postgres.database.azure.com;Database=[database_name];Port=5432;User Id=[managed_identity_name]'
