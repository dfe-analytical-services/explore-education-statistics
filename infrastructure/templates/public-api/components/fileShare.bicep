import { responseTimeConfig } from 'alerts/config.bicep'

@description('Size in GB of the file share')
param fileShareQuota int = 6

@description('Name of the file share')
param fileShareName string

@description('Type of the file share access tier')
@allowed(['Cool','Hot','Premium','TransactionOptimized'])
param fileShareAccessTier string = 'Hot'

@description('Name of the Storage Account')
param storageAccountName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  availability: bool
  latency: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

// Reference an existing Storage Account.
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource fileService 'Microsoft.Storage/storageAccounts/fileServices@2023-05-01' = {
  name: 'default'
  parent: storageAccount
}

resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-05-01' = {
  name:  fileShareName
  parent: fileService
  properties: {
    accessTier: fileShareAccessTier
    shareQuota: fileShareQuota
  }
}

module availabilityAlerts 'alerts/fileServices/availabilityAlert.bicep' = if (alerts != null && alerts!.availability) {
  name: '${storageAccountName}FsAvailabilityDeploy'
  params: {
    resourceName: storageAccountName
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module latencyAlert 'alerts/dynamicMetricAlertNew.bicep' = if (alerts != null && alerts!.latency) {
  name: '${storageAccountName}FsLatencyDeploy'
  params: {
    resourceName: '${storageAccountName}-fs'
    id: resourceId('Microsoft.Storage/storageAccounts/fileServices', storageAccountName, 'default')
    resourceMetric: {
      resourceType: 'Microsoft.Storage/storageAccounts/fileServices'
      metric: 'SuccessE2ELatency'
    }
    config: responseTimeConfig
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

output fileShareName string = fileShare.name
