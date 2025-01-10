import { responseTimeConfig } from 'alerts/dynamicAlertConfig.bicep'
import { staticAverageLessThanHundred, staticMaxGreaterThanZero, staticAverageGreaterThanZero } from 'alerts/staticAlertConfig.bicep'
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

var alertResourceName = '${storageAccountName}-fs'
var fileServiceId = resourceId('Microsoft.Storage/storageAccounts/fileServices', storageAccountName, 'default')

module availabilityAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.availability) {
  name: '${storageAccountName}FsAvailabilityAlertModule'
  params: {
    resourceName: alertResourceName
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

module latencyAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.latency) {
  name: '${storageAccountName}FsLatencyDeploy'
  params: {
    resourceName: alertResourceName
    id: fileServiceId
    resourceMetric: {
      resourceType: 'Microsoft.Storage/storageAccounts/fileServices'
      metric: 'SuccessE2ELatency'
    }
    config: responseTimeConfig
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module fileCapacity85Alert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.capacity) {
  name: '${storageAccountName}FsCapacity85Deploy'
  params: {
    resourceName: alertResourceName
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
      nameSuffix: 'file-service-85-capacity'
      threshold: string(percentage(gbsToBytes(fileShareQuotaGbs), 85))
      windowSize: 'PT1H'
      severity: 'Informational'
    }
    fullDescription: 'File service is at 85% of its reserved capacity.  Consider raising its quota soon.'
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module fileCapacity95Alert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.capacity) {
  name: '${storageAccountName}FsCapacityDeploy'
  params: {
    resourceName: alertResourceName
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
      nameSuffix: 'file-service-95-capacity'
      threshold: string(percentage(gbsToBytes(fileShareQuotaGbs), 95))
      windowSize: 'PT1H'
      severity: 'Warning'
    }
    fullDescription: 'File service is at 95% of its reserved capacity.  Raise its quota immediately.'
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

output fileShareName string = fileShare.name
