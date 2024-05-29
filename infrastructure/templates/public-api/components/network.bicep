@description('Subscription Name e.g. s101d01. Used as a prefix for created resources')
param subscription string

@description('Environment Name Used as a prefix for created resources')
param environment string 

@description('Specifies the location for all resources.')
param location string

@description('Virtual Network Address Prefix')
param vNetAddressPrefix string = '10.0.0.0/16'

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var vNetName = '${subscription}-vnet-${environment}'

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' = {
  name: vNetName
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
output vNetRef string = resourceId('Microsoft.Network/VirtualNetworks', vNetName)
