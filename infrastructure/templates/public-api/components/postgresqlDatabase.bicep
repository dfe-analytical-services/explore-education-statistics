@description('Specifies the Subscription to be used.')
param subscription string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Server Name for Azure Database for PostgreSQL')
param serverName string = 'metadata'

@description('Database administrator login name')
@minLength(1)
param administratorLoginName string

@description('Database administrator password')
@minLength(8)
@secure()
param administratorLoginPassword string

@description('Azure Database for PostgreSQL sku name, typically, tier + family + cores, e.g. Standard_D4s_v3 ')
param skuName string

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
param skuTier string = 'Burstable'

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
param KeyVaultName string

//Passed in Tags
param departmentName string = 'Public API'
param environmentName string = 'Development'
param solutionName string = 'API'
param subscriptionName string = 'Unknown'
param costCentre string = 'Unknown'
param serviceOwnerName string = 'Unknown'
param dateProvisioned string = utcNow('u')
param createdBy string = 'Unknown'
param deploymentRepo string = 'N/A'
param deploymentScript string = 'N/A'

// Variables and created data
var metadataDatabaseName = '${subscription}-postgre-${serverName}'
var pythonDbConnectionString = 'database=<database>, user=${administratorLoginName}@${metadataDatabase.name}, host=${metadataDatabase.name}${environment().suffixes.sqlServerHostname}, password=${administratorLoginPassword}, port=5432'
var adoNetDbConnectionString = 'Server=${metadataDatabase.name}${environment().suffixes.sqlServerHostname};Database=<database>;Port=5432;User Id=${administratorLoginName}@${metadataDatabase.name};Password=${administratorLoginPassword};'

var pythonSecretName = 'pythonConnectionString'
var adoNetSecretName = 'adoConnectionString'


//Resources 
resource metadataDatabase 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: metadataDatabaseName
  location: location
  sku: {
    name: skuName
    tier: skuTier
  }
  properties: {
    createMode: 'Default'
    version: postgresqlVersion
    administratorLogin: administratorLoginName
    administratorLoginPassword: administratorLoginPassword
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
  tags: {
    Department: departmentName
    Solution: solutionName
    ServiceType: 'PostgreSQL Database'
    Environment: environmentName
    Subscription: subscriptionName
    CostCentre: costCentre
    ServiceOwner: serviceOwnerName
    DateProvisioned: dateProvisioned
    CreatedBy: createdBy
    DeploymentRepo: deploymentRepo
    DeploymentScript: deploymentScript
  }
}


//store connections strings
module StorePythonConnectionStringToKeyVault './keyVaultSecret.bicep' = {
  name: 'python-kv-connectionString'
  params: {
    KeyVaultName: KeyVaultName
    IsEnabled: true
    SecretValue: pythonDbConnectionString 
    ContentType: 'text/plain'
    SecretName: pythonSecretName
  }
}

module StoreADOConnectionStringToKeyVault './keyVaultSecret.bicep' = {
  name: 'ado-kv-connectionString'
  params: {
    KeyVaultName: KeyVaultName
    IsEnabled: true
    SecretValue: adoNetDbConnectionString 
    ContentType: 'text/plain'
    SecretName: adoNetSecretName
  }
}




//Outputs
@description('The fully qualified Azure resource ID of the Database Server.')
output metadataDatabaseRef string = resourceId('Microsoft.DBforPostgreSQL/flexibleServers', metadataDatabaseName)
@description('Connection String Secrets.')
output pythonConnectionStringSecretName string = pythonSecretName
output pythonConnectionStringSecretUri string = StorePythonConnectionStringToKeyVault.outputs.SecretUri

output adoConnectionStringSecretName string = adoNetSecretName
output adoConnectionStringSecretUri string = StoreADOConnectionStringToKeyVault.outputs.SecretUri
output adoNetDbConnectionString string = adoNetDbConnectionString
