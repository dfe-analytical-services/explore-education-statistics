import { abbreviations } from '../common/abbreviations.bicep'
import { FrontDoorCertificateType } from '../common/components/front-door/types.bicep'

@description('Environment : Subscription name. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Tagging : Environment name e.g. Development. Used for tagging resources created by this infrastructure pipeline.')
param environmentName string

@description('The public site URL for use with Azure Front Door.')
param publicSiteUrl string = ''

@description('FQDN of the service hosting the public site, rather than the public URL as used by custom domains.')
param publicSiteInternalServiceFqdn string

@description('The base public URL for accessing the public API and its documentation site. This is the base FaUAPI URL that users use to interact with the API.')
param publicApiPublicUrl string

@description('FQDN of the public API listener in Application Gateway. This is the FQDN that FaUAPI uses to proxy traffic to the public API.')
param publicApiApplicationGatewayFqdn string = ''

@description('Certificate type for Azure Front Door.')
param certificateType FrontDoorCertificateType = 'BringYourOwn'

@description('Whether or not to create role assignments necessary for performing certain backup actions.')
param deployBackupVaultReaderRoleAssignment bool = true

@description('Whether or not to deploy subnets.')
param deploySubnets bool = false

@description('Whether or not to deploy Azure Front Door.')
param deployAzureFrontDoor bool = false

@description('Whether or not to deploy Application Gateway and its configuration.')
param deployApplicationGateway bool = false

@description('The minimum average response time from the public site (via Azure Front Door) before latency alerts fire.')
param averagePublicSiteResponseTimeAlertThresholdMillis int = 2500

@description('Do Azure Monitor alerts need creating or updating?')
param deployAlerts bool = false

@description('Tagging : Used for tagging resources created by this infrastructure pipeline.')
param resourceTags {
  CostCentre: string
  Department: string
  Solution: string
  ServiceOwner: string
  CreatedBy: string
  DeploymentRepoUrl: string
}?

@description('Tagging : Date Provisioned. Used for tagging resources created by this infrastructure pipeline.')
param dateProvisioned string = utcNow('u')

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

var resourcePrefix = '${subscription}-ees'
var publicApiResourcePrefix = '${subscription}-ees-papi'

var resourceNames = {
  existingResources: {
    adminApp: '${subscription}-as-ees-admin'
    publicApiApp: '${publicApiResourcePrefix}-${abbreviations.appContainerApps}-api'
    publicApiDocsApp: '${publicApiResourcePrefix}-${abbreviations.staticWebApps}-docs'
    publicSiteApp: '${subscription}-as-ees-public-site'
    publisherFunction: '${subscription}-${abbreviations.webSitesFunctions}-ees-publisher'
    alertsGroup: '${subscription}-${abbreviations.insightsActionGroups}-ees-alertedusers'
  }
}

module vNetModule 'application/virtual-network/virtual-network.bicep' = {
  name: 'vNetModuleDeploy'
  params: {
    subscription: subscription
    deploySubnets: deploySubnets
    tagValues: tagValues
  }
}

module keyVaultModule 'application/keyVault/keyVault.bicep' = {
  name: 'keyVaultModuleDeploy'
  params: {
    subscription: subscription
    tagValues: tagValues
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: '${resourcePrefix}-log'
  scope: resourceGroup()
}

module backupsModule 'application/backups/backups.bicep' = {
  name: 'backupsModuleDeploy'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    deployBackupVaultReaderRoleAssignment: deployBackupVaultReaderRoleAssignment
    tagValues: tagValues
  }
}

module eventMessagingModule 'application/eventMessaging/eventMessaging.bicep' = {
  name: 'eventMessagingModuleDeploy'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    resourceNames: {
      adminApp: resourceNames.existingResources.adminApp
      alertsGroup: resourceNames.existingResources.alertsGroup
      publisherFunction: resourceNames.existingResources.publisherFunction
      vNet: vNetModule.outputs.vNetName
      subnets: {
        eventGridCustomTopicPrivateEndpoints: vNetModule.outputs.eventGridCustomTopicPrivateEndpointsSubnetName
      }
    }
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module frontDoorModule 'application/frontDoor/frontDoor.bicep' = if (deployAzureFrontDoor) {
  name: 'frontDoorModuleDeploy'
  params: {
    subscription: subscription
    keyVaultName: keyVaultModule.outputs.keyVaultName
    resourcePrefix: resourcePrefix
    publicSiteUrl: publicSiteUrl
    certificateType: certificateType
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
    averagePublicSiteResponseTimeAlertThresholdMillis: averagePublicSiteResponseTimeAlertThresholdMillis
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module appGatewayModule 'application/applicationGateway/appGateway.bicep' = if (deployApplicationGateway) {
  name: 'appGatewayModuleDeploy'
  params: {
    location: location
    subscription: subscription
    alertsGroupName: resourceNames.existingResources.alertsGroup
    keyVaultName: keyVaultModule.outputs.keyVaultName
    publicApiAppName: resourceNames.existingResources.publicApiApp
    publicApiDocsAppName: resourceNames.existingResources.publicApiDocsApp
    publicSiteAppServiceName: resourceNames.existingResources.publicSiteApp
    vnetName: vNetModule.outputs.vNetName
    publicApiAppGatewayFqdn: publicApiApplicationGatewayFqdn
    publicApiPublicUrl: publicApiPublicUrl
    publicSiteFqdn: replace(publicSiteUrl, 'https://', '')
    publicSiteInternalServiceFqdn: publicSiteInternalServiceFqdn
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}
