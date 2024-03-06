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
])
param postgresqlVersion string = '14'

@description('PostgreSQL Server backup retention days')
param backupRetentionDays int = 7

@description('Geo-Redundant Backup setting')
param geoRedundantBackup string = 'Disabled'

@description('The name of the Key Vault to store the connection strings')
param keyVaultName string

@description('Specifies the VNet id')
param vNetId string

@description('Specifies the subnet id')
param subnetId string

@description('An array of database names')
param databaseNames array

@description('An array of firewall rules containing IP address ranges')
param firewallRules {
  name: string
  startIpAddress: string
  endIpAddress: string
}[] = []

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var databaseServerName = empty(serverName)
  ? '${resourcePrefix}-psql'
  : '${resourcePrefix}-psql-${serverName}'

var connectionStringSecretName = '${databaseServerName}-connectionString'
var connectionString = 'Server=${postgreSQLDatabase.name}${az.environment().suffixes.sqlServerHostname};${adminName}Database=<database>;Port=5432;${postgreSQLDatabase.name}User Id=${adminPassword};'

// In order to link PostgreSQL Flexible Server to a VNet, it must have a Private DNS zone available with a name ending
// with "postgres.database.azure.com".
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'private.postgres.database.azure.com'
  location: 'global'
  resource vNetLink 'virtualNetworkLinks' = {
    name: '${databaseServerName}-vnet-link'
    location: 'global'
    properties: {
      registrationEnabled: false
      virtualNetwork: {
        id: vNetId
      }
    }
  }
}

resource postgreSQLDatabase 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: databaseServerName
  location: location
  sku: {
    name: dbSkuName
    tier: dbSkuTier
  }
  properties: {
    createMode: 'Default'
    version: postgresqlVersion
    administratorLogin: adminName
    administratorLoginPassword: adminPassword
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
    network: {
      delegatedSubnetResourceId: subnetId
      privateDnsZoneArmResourceId: privateDnsZone.id
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

// Store the connections string in Key Vault and output the URI to the generated secret.
module storeADOConnectionStringToKeyVault './keyVaultSecret.bicep' = {
  name: 'dbConnectionStringSecretDeploy'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretValue: connectionString 
    contentType: 'text/plain'
    secretName: connectionStringSecretName
  }
}

@description('The fully qualified Azure resource ID of the Database Server.')
output databaseRef string = resourceId('Microsoft.DBforPostgreSQL/flexibleServers', databaseServerName)

@description('Connection String Secrets.')
output connectionStringSecretName string = connectionStringSecretName
