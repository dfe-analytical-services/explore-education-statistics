import { ResourceNames, IpRange, PrincipalNameAndId, PostgreSqlFlexibleServerConfig } from '../../types.bicep'
import { replaceMultiple } from '../../functions.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Administrator login name.')
param adminName string

@description('Administrator password.')
@secure()
param adminPassword string

@description('Server configuration.')
param serverConfig PostgreSqlFlexibleServerConfig

@description('Firewall rules.')
param firewallRules IpRange[] = []

@description('Specifies the subnet id that the PostgreSQL private endpoint will be attached to.')
param privateEndpointSubnetId string

@description('An array of Entra ID admin principal names for this resource')
param entraIdAdminPrincipals PrincipalNameAndId[] = []

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Whether to register this server with Backup Vault')
param deployBackupVaultRegistration bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var formattedFirewallRules = map(firewallRules, rule => {
  name: replace(rule.name, ' ', '_')
  cidr: rule.cidr
})

var serverName = resourceNames.sharedResources.postgreSqlFlexibleServer

// Our deploy SPN currently does not have permission to assign this role. 
var deployBackupVaultRoleAssignment = false
func createManagedIdentityConnectionString(templateString string, identityName string) string =>
  replaceMultiple(templateString, {
    '[database_name]': 'public_data'
    '[managed_identity_name]': identityName
  })

module postgreSqlServerModule '../../components/postgreSqlFlexibleServer.bicep' = {
  name: 'postgreSQLDatabaseDeploy'
  params: {
    databaseServerName: serverName
    location: location
    createMode: 'Default'
    adminName: adminName
    adminPassword: adminPassword
    entraIdAdminPrincipals: entraIdAdminPrincipals
    serverConfig: serverConfig
    firewallRules: formattedFirewallRules
    databaseNames: ['public_data']
    privateEndpointSubnetId: privateEndpointSubnetId
    alerts: deployAlerts ? {
      availability: true
      queryTime: true
      transactionTime: true
      clientConenctionsWaiting: true
      cpuPercentage: true
      diskBandwidth: true
      diskIops: true
      memoryPercentage: true
      capacity: true
      failedConnections: true
      deadlocks: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    tagValues: tagValues
  }
}

resource backupVault 'Microsoft.DataProtection/backupVaults@2022-05-01' existing = {
  name: resourceNames.existingResources.backupVault.vault
}

module backupVaultRoleAssignmentModule '../../../common/components/psql-flexible-server/roleAssignment.bicep' = if (deployBackupVaultRoleAssignment) {
  name: '${serverName}BackupVaultRoleAssignmentDeploy'
  params: {
    psqlFlexibleServerName: postgreSqlServerModule.outputs.serverName
    principalIds: [backupVault.identity.principalId]
    role: 'PostgreSQL Flexible Server Long Term Retention Backup Role'
  }
}

module backupInstanceModule '../../../common/components/data-protection/backupVaultInstance.bicep' = if (deployBackupVaultRegistration) {
  name: '${resourceNames.sharedResources.postgreSqlFlexibleServer}BackupInstanceDeploy'
  params: {
    vaultName: resourceNames.existingResources.backupVault.vault
    dataSourceType: 'psqlFlexibleServer'
    resourceId: postgreSqlServerModule.outputs.databaseRef
    resourceLocation: location
    backupPolicyName: resourceNames.existingResources.backupVault.psqlFlexibleServerBackupPolicy
    tagValues: tagValues
  }
}

var connectionStringTemplate = postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate

var dataProcessorPsqlConnectionStringSecretKey = 'ees-publicapi-data-processor-connectionstring-publicdatadb'

module storeDataProcessorPsqlConnectionString '../../components/keyVaultSecret.bicep' = {
  name: 'storeDataProcessorPsqlConnectionString'
  params: {
    keyVaultName: resourceNames.existingResources.keyVault
    secretName: dataProcessorPsqlConnectionStringSecretKey
    secretValue: createManagedIdentityConnectionString(
      connectionStringTemplate,
      resourceNames.publicApi.dataProcessorIdentity
    )
  }
}

var publisherPsqlConnectionStringSecretKey = 'ees-publisher-connectionstring-publicdatadb'

module storePublisherPsqlConnectionString '../../components/keyVaultSecret.bicep' = {
  name: 'storePublisherPsqlConnectionString'
  params: {
    keyVaultName: resourceNames.existingResources.keyVault
    secretName: publisherPsqlConnectionStringSecretKey
    secretValue: createManagedIdentityConnectionString(
      connectionStringTemplate,
      resourceNames.existingResources.publisherFunction
    )
  }
}

var adminPsqlConnectionStringSecretKey = 'ees-admin-connectionstring-publicdatadb'

module storeAdminPsqlConnectionString '../../components/keyVaultSecret.bicep' = {
  name: 'storeAdminPsqlConnectionString'
  params: {
    keyVaultName: resourceNames.existingResources.keyVault
    secretName: adminPsqlConnectionStringSecretKey
    secretValue: createManagedIdentityConnectionString(
      connectionStringTemplate,
      resourceNames.existingResources.adminApp
    ) 
  }
}

output managedIdentityConnectionStringTemplate string = connectionStringTemplate
