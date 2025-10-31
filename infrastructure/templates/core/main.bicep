@description('Environment : Used as a prefix for created resources.')
@allowed([
  's101d01'
  's101t01'
  's101p02'
  's101p01'
  '' // To avoid bicep linting errors, because the variable is fetched from DevOps Pipelines Library
])
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Environment : Used for tagging resources, and creating environment-specific resources')
@allowed([
  'Development'
  'Test'
  'Pre-Production'
  'Production'
])
param environmentName string

@description('Whether or not to create role assignments necessary for performing certain backup actions.')
param deployBackupVaultReaderRoleAssignment bool = false

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

module backupsModule 'application/backups/backups.bicep' = {
  name: 'backupsModuleDeploy'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    deployBackupVaultReaderRoleAssignment: deployBackupVaultReaderRoleAssignment
    tagValues: tagValues
  }
}

resource ddosProtectionPlan 'Microsoft.Network/ddosProtectionPlans@2019-02-01' = if (environmentName == 'Development') {
  name: 'ees-common-ddos-protection-plan'
  location: location
  tags: tagValues
}
