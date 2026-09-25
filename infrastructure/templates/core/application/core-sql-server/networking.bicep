import { VNetSubnets } from '../virtual-network/types.bicep'

@description('Name of the SQL Server that this networking configuration belongs to.')
param sqlServerName string

@description('Specifies the location for this resource.')
param location string

@description('Whether or not public access is enabled for the SQL Server. Firewall and VNet rules only apply when this is Enabled.')
param sqlServerPublicNetworkAccess 'Enabled' | 'Disabled'

@description('Firewall rules for the SQL Server, applied when public network access is enabled.')
param firewallRules {
  name: string
  startIpAddress: string
  endIpAddress: string
}[]

@description('Subnets from the VNet.')
param subnets VNetSubnets

@description('Tags for the resources')
param tagValues object

resource sqlServer 'Microsoft.Sql/servers@2025-01-01' existing = {
  name: sqlServerName
}

resource firewallRuleResources 'Microsoft.Sql/servers/firewallRules@2025-01-01' = [
  for rule in firewallRules: if (sqlServerPublicNetworkAccess == 'Enabled') {
    parent: sqlServer
    name: rule.name
    properties: {
      startIpAddress: rule.startIpAddress
      endIpAddress: rule.endIpAddress
    }
  }
]

var sqlAllowedSubnets = [
  subnets.admin
  subnets.importer
  subnets.publisher
  subnets.content
  subnets.data
  subnets.notify
  subnets.publicApiDataProcessor
]

resource virtualNetworkRuleResources 'Microsoft.Sql/servers/virtualNetworkRules@2025-01-01' = [
  for allowedSubnet in sqlAllowedSubnets: if (sqlServerPublicNetworkAccess == 'Enabled') {
    parent: sqlServer
    name: allowedSubnet.name
    properties: {
      virtualNetworkSubnetId: allowedSubnet.id
      ignoreMissingVnetServiceEndpoint: false
    }
  }
]

module privateEndpointModule '../../../common/components/privateEndpoint.bicep' = {
  name: 'coreSqlServerPrivateEndpointDeploy'
  params: {
    serviceId: sqlServer.id
    serviceName: sqlServerName
    serviceType: 'azureSql'
    subnetId: subnets.sqlServerPrivateEndpoints.id
    location: location
    tagValues: tagValues
  }
}
