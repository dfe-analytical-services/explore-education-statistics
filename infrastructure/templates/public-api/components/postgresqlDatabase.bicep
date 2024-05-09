@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

@description('Server Name for Azure Database for PostgreSQL')
param serverName string = ''

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
@allowed([
  'Burstable'
  'GeneralPurpose'
  'MemoryOptimized'
])
param dbSkuTier string = 'Burstable'

@description('PostgreSQL version')
@allowed([
  '11'
  '12'
  '13'
  '14'
  '15'
  '16'
])
param postgreSqlVersion string = '16'

@description('PostgreSQL Server backup retention days')
param backupRetentionDays int = 7

@description('Geo-Redundant Backup setting')
param geoRedundantBackup string = 'Disabled'

@description('An array of database names')
param databaseNames string[]

@description('An array of firewall rules containing IP address ranges')
param firewallRules {
  name: string
  startIpAddress: string
  endIpAddress: string
}[] = []

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

@description('The id of the VNet to which this database server will be connected via a private endpoint')
param vnetId string

@description('The id of the subnet which will be used to install the private endpoint for allowing secure connection to the database server over the VNet')
param subnetId string

var databaseServerName = empty(serverName)
  ? '${resourcePrefix}-psql-flexibleserver'
  : '${resourcePrefix}-psql-flexibleserver-${serverName}'

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

  resource rules 'firewallRules' = [for rule in firewallRules: {
    name: rule.name
    properties: {
      startIpAddress: rule.startIpAddress
      endIpAddress: rule.endIpAddress
    }
  }]

  tags: tagValues
}

var privateLinkDnsZoneName = 'privatelink.postgres.database.azure.com'

var privateEndpointName = '${databaseServerName}-plink'

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2021-05-01' = {
  name: privateEndpointName
  location: location
  properties: {
    subnet: {
      id: subnetId
    }
    privateLinkServiceConnections: [
      {
        name: privateEndpointName
        properties: {
          privateLinkServiceId: postgreSQLDatabase.id
          groupIds: [
            'postgresqlServer'
          ]
        }
      }
    ]
  }
}

resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: privateLinkDnsZoneName
  location: 'global'
  properties: {}
}

resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZone
  name: '${privateLinkDnsZoneName}-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnetId
    }
  }
}

resource privateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2021-05-01' = {
  name: 'default'
  parent: privateEndpoint
  properties: {
    privateDnsZoneConfigs: [
      {
        name: replace(privateLinkDnsZoneName, '.', '-')
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
}

@description('The fully qualified Azure resource ID of the Database Server.')
output databaseRef string = resourceId('Microsoft.DBforPostgreSQL/flexibleServers', databaseServerName)
