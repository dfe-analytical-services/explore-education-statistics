import { getResourceNamesForEnvironment } from '../common/resource-names.bicep'
import { IpRange } from '../common/types.bicep'

@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Tagging : Environment name e.g. Development. Used for tagging resources created by this infrastructure pipeline.')
param environmentName string

@description('Whether to use the naming convention for the DfE Enterprise Landing Zone (ELZ) subscriptions. Set to true by the param files in "parameters/elz".')
param elz bool = false

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

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[] = []

@description('The cron schedule for the Public API queries consumer Function. Defaults to every hour.')
param analyticsRequestFilesConsumerCron string = '0 0 * * * *'

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

var maintenanceFirewallRules = [
  for maintenanceIpRange in maintenanceIpRanges: {
    name: maintenanceIpRange.name
    cidr: maintenanceIpRange.cidr
    tag: 'Default'
    priority: 100
  }
]

var resourceNames = getResourceNamesForEnvironment({
  environmentIdentifier: subscription
  environmentName: environmentName
  elz: elz
})

// Create an Application Insights resource for Analytics resources to use.
module applicationInsightsModule 'application/analyticsApplicationInsights.bicep' = {
  name: 'analyticsApplicationInsightsModule'
  params: {
    location: location
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

module analyticsStorageModule 'application/analyticsStorage.bicep' = {
  name: 'analyticsStorageAccountApplicationModuleDeploy'
  params: {
    resourceNames: resourceNames
    storageFirewallRules: maintenanceIpRanges
    deployAlerts: true
    tagValues: tagValues
  }
}

module analyticsStorageBackupModule '../common/components/recovery-services-vault/file-share-registration.bicep' = {
  name: 'analyticsStorageBackupModuleDeploy'
  params: {
    vaultName: resourceNames.backups.recoveryServicesVault
    backupPolicyName: 'DailyPolicy'
    storageAccountName: analyticsStorageModule.outputs.storageAccountName
    fileShareName: analyticsStorageModule.outputs.fileShareName
    tagValues: tagValues
  }
}

module analyticsFunctionAppModule 'application/analyticsFunctionApp.bicep' = {
  name: 'analyticsFunctionAppModule'
  params: {
    resourceNames: resourceNames
    functionAppFirewallRules: union(
      [
        {
          cidr: 'AzureCloud'
          tag: 'ServiceTag'
          priority: 101
          name: 'AzureCloud'
        }
      ],
      maintenanceFirewallRules
    )
    storageFirewallRules: maintenanceIpRanges
    applicationInsightsConnectionString: applicationInsightsModule.outputs.applicationInsightsConnectionString
    analyticsStorageAccountName: analyticsStorageModule.outputs.storageAccountName
    analyticsFileShareName: analyticsStorageModule.outputs.fileShareName
    analyticsRequestFilesConsumerCron: analyticsRequestFilesConsumerCron
    tagValues: tagValues
  }
}

output analyticsFunctionAppUrl string = analyticsFunctionAppModule.outputs.functionAppUrl
