import { JourneyAlertPolicy } from 'journeyAlertTypes.bicep'
import { severityMapping } from 'types.bicep'

param policy JourneyAlertPolicy
param workspaceId string
param alertsGroupName string
param location string = resourceGroup().location
param tagValues object = {}

var sourceQuery = policy.kind == 'adminSignIn' ? loadTextContent('queries/adminSignIn.kql') : loadTextContent('queries/http5xx.kql')
// JSON serialization avoids interpolating unescaped path/role strings into KQL literals.
var query = '${replace(sourceQuery, '__POLICY__', string(policy))}\n${loadTextContent('queries/threshold.kql')}'

resource rule 'Microsoft.Insights/scheduledQueryRules@2023-12-01' = {
  name: policy.name
  location: location
  kind: 'LogAlert'
  tags: tagValues
  properties: {
    displayName: policy.name
    description: '${policy.description} Policy: ${policy.trigger}; count >= ${policy.errorCountThreshold}; rate >= ${policy.errorRateBasisPoints} basis points with requests >= ${policy.minimumRequests} and errors >= ${policy.minimumErrors}; window ${policy.windowSize}. Review query results for counts and cohort.'
    enabled: policy.enabled
    severity: severityMapping[policy.severity]
    scopes: [workspaceId]
    evaluationFrequency: 'PT5M'
    windowSize: policy.windowSize
    autoMitigate: true
    skipQueryValidation: false
    criteria: {
      allOf: [{
        query: query
        metricMeasureColumn: 'Breach'
        timeAggregation: 'Maximum'
        operator: 'GreaterThan'
        threshold: 0
        failingPeriods: {
          minFailingPeriodsToAlert: 1
          numberOfEvaluationPeriods: 1
        }
      }]
    }
    actions: {
      actionGroups: [resourceId('Microsoft.Insights/actionGroups', alertsGroupName)]
    }
  }
}
