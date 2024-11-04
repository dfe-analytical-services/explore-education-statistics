import { resourceNamesType, appGatewaySiteConfigType } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames resourceNamesType

@description('Specifies the location for all resources.')
param location string

@description('Specifies the subnet id for the App Gateway to integrate with the VNet.')
param publicApiContainerAppSettings appGatewaySiteConfigType

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.appGateway
  parent: vNet
}

module appGatewayModule '../../components/appGateway.bicep' = {
  name: 'appGatewayDeploy'
  params: {
    location: location
    appGatewayName: resourceNames.sharedResources.appGateway
    managedIdentityName: resourceNames.sharedResources.appGatewayIdentity
    keyVaultName: resourceNames.existingResources.keyVault
    subnetId: subnet.id
    sites: [
      publicApiContainerAppSettings
    ]
    tagValues: tagValues
  }
}
