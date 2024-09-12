import { firewallRuleType } from '../types.bicep'

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
param firewallRules firewallRuleType[] = []

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
resource rules 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2022-12-01' = [for rule in firewallRules: {
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

@description('The fully qualified Azure resource ID of the Database Server.')
output databaseRef string = resourceId('Microsoft.DBforPostgreSQL/flexibleServers', databaseServerName)

@description('A template connection string to be used with managed identities and access tokens.')
output managedIdentityConnectionStringTemplate string = 'Server=${postgreSQLDatabase.name}.postgres.database.azure.com;Database=[database_name];Port=5432;User Id=[managed_identity_name]'
