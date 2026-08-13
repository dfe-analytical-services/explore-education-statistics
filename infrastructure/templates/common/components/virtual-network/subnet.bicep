import { Subnet } from 'types.bicep'

@description('Name of the owning VNet.')
param vNetName string

@description('Configuration for this subnet.')
param config Subnet

// Maintain a dictionary of simple delegation types to fully-formed config.
var delegationDictionary = {
  webapp: {
    name: 'webapp'
    properties: {
      serviceName: 'Microsoft.Web/serverFarms'
    }
    type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
  }
  environment: {
    name: 'environment'
    properties: {
      serviceName: 'Microsoft.App/environments'
    }
    type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
  }
}

// Swap simple service endpoint names for fully-formed service endpoint config.
var serviceEndpoints = map(config.properties.?serviceEndpoints ?? [], serviceEndpoint => {
  service: serviceEndpoint
})

// Swap simple delegation names for fully-formed delegation config.
var delegations = map(config.properties.?delegations ?? [], delegationType => delegationDictionary[delegationType])

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2025-07-01' = {
  name: '${vNetName}/${config.name}'
  properties: {
    ...config.properties
    serviceEndpoints: serviceEndpoints
    delegations: delegations
  }
}
