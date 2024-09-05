@description('Specifies the location for all resources.')
param location string

@description('Specifies common resource prefix.')
param commonResourcePrefix string

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

var appGatewayName = '${commonResourcePrefix}-agw-01'
var appGatewayIdentityName = '${commonResourcePrefix}-id-agw-01'

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
