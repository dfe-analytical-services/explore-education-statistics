import { severityMapping } from 'types.bicep'
import { DynamicAlertConfig } from 'dynamicAlertConfig.bicep'

import { ResourceMetric } from 'resourceMetrics.bicep'

@description('Name of the resource that this alert is being applied to.')
param resourceName string

@description('''
Optional id of the resource that this alert is being applied to, 
if it cannot be looked up by the combination of resourceName and resourceType.
''')
param id string?

@description('Resource type and metric name combination.')
param resourceMetric ResourceMetric

@description('Configuration for this alert.')
param config DynamicAlertConfig

@description('Optional description of alert.')
param fullDescription string?

@description('''
An optional date that prevents machine learning algorithms from using metric data prior to this date in order to
calculate its dynamic threshold.
''')
param ignoreDataBefore string?

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

var severityLevel = severityMapping[config.severity]

var resourceIds = [id != null ? id! : resourceId(resourceMetric.resourceType, resourceName)]

resource alertsActionGroup 'Microsoft.Insights/actionGroups@2023-01-01' existing = {
  name: alertsGroupName
}

resource metricAlertRule 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${resourceName}-${config.nameSuffix}'
  location: 'Global'
  properties: {
    enabled: true
    scopes: resourceIds
    targetResourceType: resourceMetric.resourceType
    severity: severityLevel
    evaluationFrequency: config.evaluationFrequency
    windowSize: config.windowSize
    description: fullDescription
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'DynamicThresholdCriterion'
          name: 'Metric1'
          metricName: resourceMetric.metric
          metricNamespace: resourceMetric.resourceType
          timeAggregation: config.aggregation
          operator: config.operator
          alertSensitivity: config.sensitivity
          failingPeriods: {
            numberOfEvaluationPeriods: config.evaluationPeriods
            minFailingPeriodsToAlert: config.minFailingEvaluationPeriods
          }
          ignoreDataBefore: ignoreDataBefore

          dimensions: length(resourceMetric.?dimensions ?? []) > 0 ? map(resourceMetric.dimensions, dimension => {
            operator: 'Include'
            ...dimension
          }) : null

          skipMetricValidation: false
        }
      ]
    }
    actions: [
      {
        actionGroupId: alertsActionGroup.id
      }
    ]
  }
  tags: tagValues
}
