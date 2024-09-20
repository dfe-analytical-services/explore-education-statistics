@description('The name of the VNet')
param vnetName string

@description('Specifies the location for all resources.')
param location string

@description('Virtual Network Address Prefix')
param vNetAddressPrefix string = '10.0.0.0/16'

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        vNetAddressPrefix
      ]
    }
  }
  tags: tagValues
}

@description('The fully qualified Azure resource ID of the Network.')
output vNetRef string = resourceId('Microsoft.Network/VirtualNetworks', vnetName)
