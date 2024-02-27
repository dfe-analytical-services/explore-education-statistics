@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Server Name for Azure Database for PostgreSQL')
param serverName string

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

param databaseNames array
param firewallRules array

//Passed in Tags
param tagValues object

// Variables and created data
var databaseServerName = empty(serverName)
  ? '${resourcePrefix}-psql2'
  : '${resourcePrefix}-psql2-${serverName}'

var connectionStringSecretName = '${databaseServerName}-connectionString'
var connectionString = 'Server=${postgreSQLDatabase.name}${az.environment().suffixes.sqlServerHostname};${adminName}Database=<database>;Port=5432;${postgreSQLDatabase.name}User Id=${adminPassword};'

resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
    name: '${databaseServerName}.privatedns.postgres.database.azure.com'
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

//Resources
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
    name: '${rule.Name}'
    properties: {
      startIpAddress: rule.StartIpAddress
      endIpAddress: rule.EndIpAddress
    }
  }]

  tags: tagValues
}

//store connections string
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



//Outputs
@description('The fully qualified Azure resource ID of the Database Server.')
output databaseRef string = resourceId('Microsoft.DBforPostgreSQL/flexibleServers', databaseServerName)

@description('Connection String Secrets.')
output connectionStringSecretName string = connectionStringSecretName
