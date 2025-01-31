import { responseTimeConfig } from 'alerts/dynamicAlertConfig.bicep'
import { staticAverageLessThanHundred, staticMaxGreaterThanZero, staticAverageGreaterThanZero, capacity } from 'alerts/staticAlertConfig.bicep'
import { AllValuesForDimension } from 'alerts/types.bicep'
import { percentage, gbsToBytes } from '../functions.bicep'

@description('Size in GB of the file share')
param fileShareQuotaGbs int = 6

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
  capacity: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

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
    shareQuota: fileShareQuotaGbs
  }
}

var fileServiceId = resourceId('Microsoft.Storage/storageAccounts/fileServices', storageAccountName, 'default')

module availabilityAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.availability) {
  name: '${storageAccountName}FsAvailabilityAlertModule'
  params: {
    resourceName: storageAccountName
    id: fileServiceId
    resourceMetric: {
      resourceType: 'Microsoft.Storage/storageAccounts/fileServices'
      metric: 'availability'
    }
    config: {
      ...staticAverageLessThanHundred
      nameSuffix: 'file-service-availability'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module latencyAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.latency) {
  name: '${storageAccountName}FsLatencyDeploy'
  params: {
    resourceName: storageAccountName
    id: fileServiceId
    resourceMetric: {
      resourceType: 'Microsoft.Storage/storageAccounts/fileServices'
      metric: 'SuccessE2ELatency'
    }
    config: {
      ...staticAverageGreaterThanZero
      nameSuffix: 'response-time'
      threshold: '250'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

var capacityAlertThresholds = [{
  threshold: 85
  severity: 'Informational'
  description: 'File service is at 85% of its reserved capacity.  Consider raising its quota soon.'
}
{
  threshold: 90
  severity: 'Warning'
  description: 'File service is at 90% of its reserved capacity.  Raise its quota as a priority.'
}
{
  threshold: 95
  severity: 'Critical'
  description: 'File service is at 95% of its reserved capacity.  Raise its quote as soon as possible.'
}]

module fileCapacityAlerts 'alerts/staticMetricAlert.bicep' = [for capacityThreshold in capacityAlertThresholds: if (alerts != null && alerts!.capacity) {
  name: '${storageAccountName}FsCapacity${capacityThreshold.threshold}Deploy'
  params: {
    resourceName: storageAccountName
    id: fileServiceId
    resourceMetric: {
      resourceType: 'Microsoft.Storage/storageAccounts/fileServices'
      metric: 'FileCapacity'
      dimensions: [{
        name: 'FileShare'
        values: AllValuesForDimension
      }]
    }
    config: {
      ...staticAverageGreaterThanZero
      nameSuffix: 'file-service-${capacityThreshold.threshold}-capacity'
      threshold: string(percentage(gbsToBytes(fileShareQuotaGbs), capacityThreshold.threshold))
      windowSize: 'PT1H'
      severity: capacityThreshold.severity
    }
    fullDescription: capacityThreshold.description
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}]

output fileShareName string = fileShare.name
