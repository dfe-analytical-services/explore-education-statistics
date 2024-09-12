import { resourceNamesType, firewallRuleType } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames resourceNamesType

@description('Specifies the location for all resources.')
param location string

@description('Administrator login name.')
param adminName string

@description('Administrator password.')
@secure()
param adminPassword string

@description('SKU name.')
param sku string = 'Standard_B1ms'

@description('Storage Size in GB.')
param storageSizeGB int = 32

@description('Autogrow setting.')
param autoGrowStatus string = 'Disabled'

@description('Firewall rules.')
param firewallRules firewallRuleType[] = []

@description('Specifies the subnet id that the PostgreSQL private endpoint will be attached to.')
param privateEndpointSubnetId string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var formattedFirewallRules = map(firewallRules, rule => {
  name: replace(rule.name, ' ', '_')
  cidr: rule.cidr
})

module postgreSqlServerModule '../../components/postgresqlDatabase.bicep' = {
  name: 'postgreSQLDatabaseDeploy'
  params: {
    databaseServerName: resourceNames.sharedResources.postgreSqlFlexibleServer
    location: location
    createMode: 'Default'
    adminName: adminName
    adminPassword: adminPassword
    dbSkuName: sku
    dbStorageSizeGB: storageSizeGB
    dbAutoGrowStatus: autoGrowStatus
    postgreSqlVersion: '16'
    firewallRules: formattedFirewallRules
    databaseNames: ['public_data']
    privateEndpointSubnetId: privateEndpointSubnetId
    tagValues: tagValues
  }
}

@description('A template connection string to be used with managed identities and access tokens.')
output managedIdentityConnectionStringTemplate string = postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate
