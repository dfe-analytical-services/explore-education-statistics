import { ResourceNames } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

var subnets = resourceNames.existingResources.subnets

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource adminSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: subnets.adminApp
  parent: vNet
}

resource publisherSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: subnets.publisherFunction
  parent: vNet
}

resource dataProcessorSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: subnets.dataProcessor
  parent: vNet
}

resource dataProcessorPrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: subnets.dataProcessorPrivateEndpoints
  parent: vNet
}

resource containerAppEnvironmentSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: subnets.containerAppEnvironment
  parent: vNet
}

resource psqlFlexibleServerSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: subnets.psqlFlexibleServer
  parent: vNet
}

resource appGatewaySubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: subnets.appGateway
  parent: vNet
}

resource storageAccountPrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: subnets.storagePrivateEndpoints
  parent: vNet
}

@description('The fully qualified Azure resource ID of the virtual network')
output vnetId string = resourceId('Microsoft.Network/VirtualNetworks', resourceNames.existingResources.vNet)

@description('The name of the virtual network')
output vnetName string = vNet.name

@description('The fully qualified Azure resource ID of the Data Processor Function App Subnet.')
output dataProcessorSubnetRef string = dataProcessorSubnet.id

@description('The first usable IP address for the Data Processor Function App Subnet.')
output dataProcessorSubnetStartIpAddress string = parseCidr(dataProcessorSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the Data Processor Function App Subnet.')
output dataProcessorSubnetEndIpAddress string = parseCidr(dataProcessorSubnet.properties.addressPrefix).lastUsable

@description('The fully qualified Azure resource ID of the Data Processor Function App Private Endpoint Subnet.')
output dataProcessorPrivateEndpointSubnetRef string = dataProcessorPrivateEndpointSubnet.id

@description('The first usable IP address for the Data Processor Function App Private Endpoint Subnet.')
output dataProcessorPrivateEndpointSubnetStartIpAddress string = parseCidr(dataProcessorPrivateEndpointSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the Data Processor Function App Private Endpoint Subnet.')
output dataProcessorPrivateEndpointSubnetEndIpAddress string = parseCidr(dataProcessorPrivateEndpointSubnet.properties.addressPrefix).lastUsable

@description('The fully qualified Azure resource ID of the API Container App Subnet.')
output containerAppEnvironmentSubnetRef string = containerAppEnvironmentSubnet.id

@description('The first usable IP address for the API Container App Subnet.')
output containerAppEnvironmentSubnetStartIpAddress string = parseCidr(containerAppEnvironmentSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the API Container App Subnet.')
output containerAppEnvironmentSubnetEndIpAddress string = parseCidr(containerAppEnvironmentSubnet.properties.addressPrefix).lastUsable

@description('The first usable IP address for the Admin App Service Subnet.')
output adminAppServiceSubnetStartIpAddress string = parseCidr(adminSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the Admin App Service Subnet.')
output adminAppServiceSubnetEndIpAddress string = parseCidr(adminSubnet.properties.addressPrefix).lastUsable

@description('The IP address range for the Admin App Service Subnet.')
output adminAppServiceSubnetCidr string = adminSubnet.properties.addressPrefix

@description('The first usable IP address for the Publisher Function App Subnet.')
output publisherFunctionAppSubnetStartIpAddress string = parseCidr(publisherSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the Publisher Function App Subnet.')
output publisherFunctionAppSubnetEndIpAddress string = parseCidr(publisherSubnet.properties.addressPrefix).lastUsable

@description('The fully qualified Azure resource ID of the PSQL Flexible Server Subnet.')
output psqlFlexibleServerSubnetRef string = psqlFlexibleServerSubnet.id

@description('The first usable IP address for the PSQL Flexible Server Subnet.')
output psqlFlexibleServerSubnetStartIpAddress string = parseCidr(psqlFlexibleServerSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the PSQL Flexible Server Subnet.')
output psqlFlexibleServerSubnetEndIpAddress string = parseCidr(psqlFlexibleServerSubnet.properties.addressPrefix).lastUsable

@description('The fully qualified Azure resource ID of the App Gateway Subnet.')
output appGatewaySubnetRef string = appGatewaySubnet.id

@description('The fully qualified Azure resource ID of the Public API Storage Account Private Endpoint Subnet.')
output storageAccountPrivateEndpointSubnetRef string = storageAccountPrivateEndpointSubnet.id
