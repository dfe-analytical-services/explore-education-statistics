@description('Subscription Name e.g. s101d01. Used as a prefix for created resources')
param subscription string

@description('Environment Name Used as a prefix for created resources')
param environment string 

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Virtual Network Address Prefix')
param vNetAddressPrefix string = '10.0.0.0/16'

//Passed in Tags
param tagValues object

// Variables and created data
var vNetName = '${subscription}-vnet-${environment}'


//Resources 
resource vNet 'Microsoft.Network/virtualNetworks@2023-04-01' = {
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

// Outputs for exported use
@description('The fully qualified Azure resource ID of the Network.')
output vNetRef string = resourceId('Microsoft.Network/VirtualNetworks', vNetName)
