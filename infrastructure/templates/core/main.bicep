import { FrontDoorCertificateType } from 'application/frontDoor/types.bicep'

@description('Environment : Subscription name. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Tagging : Environment name e.g. Development. Used for tagging resources created by this infrastructure pipeline.')
param environmentName string

@description('The public site URL for use with Azure Front Door.')
param publicSiteUrl string = ''

@description('Certificate type for Azure Front Door.')
param certificateType FrontDoorCertificateType = 'Provisioned'

@description('Whether or not to create role assignments necessary for performing certain backup actions.')
param deployBackupVaultReaderRoleAssignment bool = true

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
var legacyResourcePrefix = '${subscription}-'

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

module frontDoorModule 'application/frontDoor/frontDoor.bicep' = {
  name: 'frontDoorModuleDeploy'
  params: {
    subscription: subscription
    resourcePrefix: resourcePrefix
    legacyResourcePrefix: legacyResourcePrefix
    publicSiteUrl: publicSiteUrl
    certificateType: certificateType
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
    tagValues: tagValues
  }
}
