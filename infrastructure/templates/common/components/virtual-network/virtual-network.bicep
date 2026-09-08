import { Subnet } from 'types.bicep'

@description('Name of the VNet.')
param vNetName string

@description('Specifies the location for all resources. Defaults to the Resource Group location.')
param location string = resourceGroup().location

@description('Address space prefix e.g. 10.0.0.0/16.')
param addressSpacePrefix string

@description('Array of subnets.')
param subnets Subnet[]?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

resource vNet 'Microsoft.Network/virtualNetworks@2025-07-01' = {
  name: vNetName
  location: location
  tags: tagValues
  properties: {
    addressSpace: {
      addressPrefixes: [
        addressSpacePrefix
      ]
    }
    privateEndpointVNetPolicies: 'Disabled'
    virtualNetworkPeerings: []
    enableDdosProtection: false
  }
}

@batchSize(1)
module subnetModules 'subnet.bicep' = [for subnet in (subnets ?? []): {
  name: '${subnet.name}ModuleDeploy'
  params: {
    vNetName: vNetName
    config: subnet
  }
  dependsOn: [
    vNet
  ]
}]
