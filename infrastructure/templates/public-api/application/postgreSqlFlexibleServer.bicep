@description('Specifies the location for all resources.')
param location string

@description('PostgreSQL Flexible Server name.')
param postgreSqlServerName string = ''

@description('Administrator login name.')
param postgreSqlAdminName string = ''

@description('Administrator password.')
@secure()
param postgreSqlAdminPassword string?

@description('SKU name.')
param postgreSqlSkuName string = 'Standard_B1ms'

@description('Storage Size in GB.')
param postgreSqlStorageSizeGB int = 32

@description('Autogrow setting.')
param postgreSqlAutoGrowStatus string = 'Disabled'

@description('Firewall rules.')
param postgreSqlFirewallRules {
  name: string
  cidr: string
}[] = []

@description('Specifies the subnet id that the PostgreSQL private endpoint will be attached to.')
param privateEndpointSubnetId string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var formattedPostgreSqlFirewallRules = map(postgreSqlFirewallRules, rule => {
  name: replace(rule.name, ' ', '_')
  cidr: rule.cidr
})

module postgreSqlServerModule '../components/postgresqlDatabase.bicep' = {
  name: 'postgreSQLDatabaseDeploy'
  params: {
    databaseServerName: postgreSqlServerName
    location: location
    createMode: 'Default'
    adminName: postgreSqlAdminName
    adminPassword: postgreSqlAdminPassword!
    dbSkuName: postgreSqlSkuName
    dbStorageSizeGB: postgreSqlStorageSizeGB
    dbAutoGrowStatus: postgreSqlAutoGrowStatus
    postgreSqlVersion: '16'
    firewallRules: formattedPostgreSqlFirewallRules
    databaseNames: ['public_data']
    privateEndpointSubnetId: privateEndpointSubnetId
    tagValues: tagValues
  }
}

@description('A template connection string to be used with managed identities and access tokens.')
output managedIdentityConnectionStringTemplate string = postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate
