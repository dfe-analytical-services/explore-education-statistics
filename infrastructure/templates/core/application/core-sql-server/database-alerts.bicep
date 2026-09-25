import { staticAverageGreaterThanZero, staticMaxGreaterThanZero, staticTotalGreaterThanZero } from '../../../common/components/alerts/staticAlertConfig.bicep'
import { fastEvaluation } from '../../../common/components/alerts/evaluation-config.bicep'

@description('Name of the database that these alerts are being applied to, e.g. "statistics" or "content".')
param resourceName string

@description('Resource id of the database that these alerts are being applied to.')
param databaseId string

@description('Name of the Action Group that receives alerts.')
param alertsGroupName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Tags for the resources')
param tagValues object

module cpuPercentAlert '../../../common/components/alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}DbCpuPercentAlertDeploy'
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

module dataIoPercentAlert '../../../common/components/alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}DbDataIoPercentAlertDeploy'
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

module failedConnectionsAlert '../../../common/components/alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}DbFailedConnectionsAlertDeploy'
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

module deadlockAlert '../../../common/components/alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}DbDeadlockAlertDeploy'
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

module dataSpaceUsedPercentAlert '../../../common/components/alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}DbDataSpaceUsedPercentAlertDeploy'
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

module dataSpaceUsedPercentUrgentAlert '../../../common/components/alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}DbDataSpaceUsedPercentUrgentAlertDeploy'
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

module blockedByFirewallAlert '../../../common/components/alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${resourceName}DbBlockedByFirewallAlertDeploy'
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
