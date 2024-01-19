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
param storageSizeGB int

@description('Azure Database for PostgreSQL Autogrow setting')
param autoGrowStatus string

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

//Passed in Tags
param tagValues object

// Variables and created data
var databaseName = '${resourcePrefix}-psql-${serverName}'
var connectionStringSecretName = '${resourcePrefix}-sql-${serverName}-connectionString'
var connectionString = 'Server=${postgreSQLDatabase.name}${az.environment().suffixes.sqlServerHostname};${adminName}Database=<database>;Port=5432;${postgreSQLDatabase.name}User Id=${adminPassword};'


//Resources 
resource postgreSQLDatabase 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: databaseName
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
      storageSizeGB: storageSizeGB
      autoGrow: autoGrowStatus
    }
    backup: {
      backupRetentionDays: backupRetentionDays
      geoRedundantBackup: geoRedundantBackup
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
  tags: tagValues
}

//store connections string
module storeADOConnectionStringToKeyVault './keyVaultSecret.bicep' = {
  name: 'connectionString'
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
output databaseRef string = resourceId('Microsoft.DBforPostgreSQL/flexibleServers', databaseName)

@description('Connection String Secrets.')
output connectionStringSecretName string = connectionStringSecretName
output connectionStringSecretUri string = storeADOConnectionStringToKeyVault.outputs.keyVaultSecretUri
output dbConnectionString string = connectionString
