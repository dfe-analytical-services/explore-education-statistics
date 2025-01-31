import { ResourceNames, IpRange, PrincipalNameAndId, PostgreSqlFlexibleServerConfig } from '../../types.bicep'

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

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var formattedFirewallRules = map(firewallRules, rule => {
  name: replace(rule.name, ' ', '_')
  cidr: rule.cidr
})

module postgreSqlServerModule '../../components/postgreSqlFlexibleServer.bicep' = {
  name: 'postgreSQLDatabaseDeploy'
  params: {
    databaseServerName: resourceNames.sharedResources.postgreSqlFlexibleServer
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

var managedIdentityConnectionStringTemplate = postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate

var dataProcessorPsqlConnectionStringSecretKey = 'ees-publicapi-data-processor-connectionstring-publicdatadb'

module storeDataProcessorPsqlConnectionString '../../components/keyVaultSecret.bicep' = {
  name: 'storeDataProcessorPsqlConnectionString'
  params: {
    keyVaultName: resourceNames.existingResources.keyVault
    isEnabled: true
    secretName: dataProcessorPsqlConnectionStringSecretKey
    secretValue: replace(replace(managedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', resourceNames.publicApi.dataProcessorIdentity)
    contentType: 'text/plain'
  }
}

var publisherPsqlConnectionStringSecretKey = 'ees-publisher-connectionstring-publicdatadb'

module storePublisherPsqlConnectionString '../../components/keyVaultSecret.bicep' = {
  name: 'storePublisherPsqlConnectionString'
  params: {
    keyVaultName: resourceNames.existingResources.keyVault
    isEnabled: true
    secretName: publisherPsqlConnectionStringSecretKey
    secretValue: replace(replace(managedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', resourceNames.existingResources.publisherFunction)
    contentType: 'text/plain'
  }
}

var adminPsqlConnectionStringSecretKey = 'ees-admin-connectionstring-publicdatadb'

module storeAdminPsqlConnectionString '../../components/keyVaultSecret.bicep' = {
  name: 'storeAdminPsqlConnectionString'
  params: {
    keyVaultName: resourceNames.existingResources.keyVault
    isEnabled: true
    secretName: adminPsqlConnectionStringSecretKey
    secretValue: replace(replace(managedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', resourceNames.existingResources.adminApp)
    contentType: 'text/plain'
  }
}

output managedIdentityConnectionStringTemplate string = managedIdentityConnectionStringTemplate
