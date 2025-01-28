import { severityMapping } from 'types.bicep'
import { StaticAlertConfig } from 'staticAlertConfig.bicep'
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
param config StaticAlertConfig

@description('Optional description of alert.')
param fullDescription string?

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
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria' 
      allOf: [{
        criterionType: 'StaticThresholdCriterion'
        name: 'Metric1'
        metricName: resourceMetric.metric
        metricNamespace: resourceMetric.resourceType
        timeAggregation: config.aggregation
        operator: config.operator
        
        // Disabling type checking against the "threshold" field as we need to be able to supply
        // thresholds that are not only ints, but decimals too.  The underlying ARM type definition
        // expects only ints.  
        #disable-next-line BCP036
        threshold: config.threshold

        dimensions: length(resourceMetric.?dimensions ?? []) > 0 ? map(resourceMetric.dimensions, dimension => {
          operator: 'Include'
            ...dimension
        }) : null

        skipMetricValidation: false
      }]
    }
    actions: [
      {
        actionGroupId: alertsActionGroup.id
      }
    ]
  }
  tags: tagValues
}
