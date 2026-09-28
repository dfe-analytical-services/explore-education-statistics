import { staticAverageGreaterThanZero, staticMaxGreaterThanZero, staticTotalGreaterThanZero } from '../alerts/staticAlertConfig.bicep'
import { fastEvaluation } from '../alerts/evaluation-config.bicep'

@description('Name used to identify the database in alert and deployment names, e.g. "s101d01-sqlsvr-ees-01-statistics".')
param resourceName string

@description('Resource id of the database that these alerts are being applied to.')
param databaseId string

@description('Name of the Action Group that receives alerts.')
param alertsGroupName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Tags for the resources')
param tagValues object

module cpuPercentAlert '../alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}CpuPercentAlertDeploy'
  params: {
    resourceName: resourceName
    id: databaseId
    resourceMetric: {
      resourceType: 'Microsoft.Sql/servers/databases'
      metric: 'cpu_percent'
    }
    config: {
      ...staticAverageGreaterThanZero
      ...fastEvaluation
      nameSuffix: 'cpu-percent'
      threshold: '85'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}

module dataIoPercentAlert '../alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}DataIoPercentAlertDeploy'
  params: {
    resourceName: resourceName
    id: databaseId
    resourceMetric: {
      resourceType: 'Microsoft.Sql/servers/databases'
      metric: 'physical_data_read_percent'
    }
    config: {
      ...staticAverageGreaterThanZero
      ...fastEvaluation
      nameSuffix: 'data-io-percent'
      threshold: '85'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}

module failedConnectionsAlert '../alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}FailedConnectionsAlertDeploy'
  params: {
    resourceName: resourceName
    id: databaseId
    resourceMetric: {
      resourceType: 'Microsoft.Sql/servers/databases'
      metric: 'connection_failed'
    }
    config: {
      ...staticTotalGreaterThanZero
      ...fastEvaluation
      nameSuffix: 'failed-connections'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}

module deadlockAlert '../alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}DeadlockAlertDeploy'
  params: {
    resourceName: resourceName
    id: databaseId
    resourceMetric: {
      resourceType: 'Microsoft.Sql/servers/databases'
      metric: 'deadlock'
    }
    config: {
      ...staticTotalGreaterThanZero
      ...fastEvaluation
      nameSuffix: 'deadlock'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}

module dataSpaceUsedPercentAlert '../alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}DataSpaceUsedPercentAlertDeploy'
  params: {
    resourceName: resourceName
    id: databaseId
    resourceMetric: {
      resourceType: 'Microsoft.Sql/servers/databases'
      metric: 'storage_percent'
    }
    config: {
      ...staticMaxGreaterThanZero
      ...fastEvaluation
      nameSuffix: 'data-space-used-percent'
      threshold: '85'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}

module dataSpaceUsedPercentUrgentAlert '../alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}DataSpaceUrgentAlertDeploy'
  params: {
    resourceName: resourceName
    id: databaseId
    resourceMetric: {
      resourceType: 'Microsoft.Sql/servers/databases'
      metric: 'storage_percent'
    }
    config: {
      ...staticMaxGreaterThanZero
      ...fastEvaluation
      nameSuffix: 'data-space-used-percent-urgent'
      threshold: '95'
      severity: 'Critical'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}

module blockedByFirewallAlert '../alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}BlockedByFirewallAlertDeploy'
  params: {
    resourceName: resourceName
    id: databaseId
    resourceMetric: {
      resourceType: 'Microsoft.Sql/servers/databases'
      metric: 'blocked_by_firewall'
    }
    config: {
      ...staticTotalGreaterThanZero
      ...fastEvaluation
      nameSuffix: 'blocked-by-firewall'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
