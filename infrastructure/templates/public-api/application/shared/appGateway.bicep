import { resourceNamesType } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames resourceNamesType

@description('Specifies the location for all resources.')
param location string

@description('Specifies the name of the Key Vault.')
param keyVaultName string

@description('Specifies the subnet id for the App Gateway to integrate with the VNet.')
param subnetId string

@description('Specifies the subnet id for the App Gateway to integrate with the VNet.')
param publicApiContainerAppSettings {
  name: string
  backendFqdn: string
  publicFqdn: string
  certificateName: string
  healthProbeRelativeUrl: string
}

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var appGatewayName = '${resourceNames.prefixes.common}-${resourceNames.abbreviations.networkApplicationGateways}-01'
var appGatewayIdentityName = '${resourceNames.prefixes.common}-${resourceNames.abbreviations.managedIdentityUserAssignedIdentities}-${resourceNames.abbreviations.networkApplicationGateways}-01'

module appGatewayModule '../../components/appGateway.bicep' = {
  name: 'appGatewayDeploy'
  params: {
    location: location
    appGatewayName: appGatewayName
    managedIdentityName: appGatewayIdentityName
    keyVaultName: keyVaultName
    subnetId: subnetId
    sites: [
      {
        resourceName: publicApiContainerAppSettings.name
        backendFqdn: publicApiContainerAppSettings.backendFqdn
        publicFqdn: publicApiContainerAppSettings.publicFqdn
        certificateKeyVaultSecretName: publicApiContainerAppSettings.certificateName
        healthProbeRelativeUrl: publicApiContainerAppSettings.healthProbeRelativeUrl
      }
    ]
    tagValues: tagValues
  }
}
