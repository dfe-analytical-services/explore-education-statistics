import { ResourceNames } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

var subnets = resourceNames.existingResources.subnets

resource vNet 'Microsoft.Network/virtualNetworks@2024-07-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource adminSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  name: subnets.adminApp
  parent: vNet
}

resource publisherSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  name: subnets.publisherFunction
  parent: vNet
}

@description('The IP address range for the Admin App Service Subnet.')
output adminAppServiceSubnetCidr string = adminSubnet.properties.addressPrefix

@description('The IP address range for the Publisher Function App Subnet.')
output publisherFunctionAppSubnetCidr string = publisherSubnet.properties.addressPrefix
