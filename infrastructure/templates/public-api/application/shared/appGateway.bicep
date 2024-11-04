import {
  AppGatewayBackend
  AppGatewayRewriteSet
  AppGatewayRoute
  AppGatewaySite
  resourceNamesType
} from '../../types.bicep'

@description('Common resource naming variables')
param resourceNames resourceNamesType

@description('The location to create resources in')
param location string

@description('Sites that the App Gateway handles')
param sites AppGatewaySite[]

@description('Backend resources the App Gateway can direct traffic to')
param backends AppGatewayBackend[]

@description('Routes the App Gateway should direct traffic through')
param routes AppGatewayRoute[]

@description('Rules for how the App Gateway should rewrite URLs')
param rewrites AppGatewayRewriteSet[]

@description('Tags for the resources')
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
    sites: sites
    backends: backends
    routes: routes
    rewrites: rewrites
    tagValues: tagValues
  }
}
