import { severityMapping, Severity, EvaluationFrequency, WindowSize } from '../alerts/types.bicep'

@description('Name of the availability test (also used as the alert resource name suffix).')
param name string

@description('Specifies the location for the availability test resource. Should match the Application Insights resource location.')
param location string

@description('Resource id of the Application Insights resource that this availability test is linked to.')
param appInsightsId string

@description('Full description of what this availability test is checking.')
param testDescription string

@description('The URL that the availability test will call.')
param url string

@description('''
Webtest location Ids from which the test will be executed.
See https://learn.microsoft.com/en-us/azure/azure-monitor/app/availability-standard-tests for the full list.
''')
param testLocations string[] = [
  'emea-gb-db3-azr' // North Europe (Dublin)
  'emea-nl-ams-azr' // West Europe (Amsterdam)
  'emea-fr-pra-edge' // France Central (Paris)
  'emea-ru-msa-edge' // UK South
  'emea-se-sto-edge' // UK West
]

@description('How often (in seconds) the test is run from each test location. Supported values are 300, 600 and 900.')
param frequencyInSeconds int = 300

@description('The number of seconds to wait for a response before the test is considered to have failed.')
param timeoutInSeconds int = 30

@description('The string that must be present in the response body for the test to be considered successful.')
param contentMatch string = 'paging'

@description('''
The number of test locations that must report a failure at the same time before the alert fires.
Requiring failures from multiple locations avoids false alarms caused by an isolated monitoring location issue.
''')
param minFailedLocationsToAlert int = 3

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('The alert severity.')
param alertSeverity Severity = 'Critical'

@description('The frequency with which the alert rule evaluates the availability test results against the threshold.')
param alertEvaluationFrequency EvaluationFrequency = 'PT5M'

@description('The timespan used to evaluate failed test location count against the threshold.')
param alertWindowSize WindowSize = 'PT5M'

@description('Flag that indicates whether the availability test and its alert are enabled.')
param enabled bool = true

@description('Whether to create or update the Azure Monitor alert for this availability test during this deploy.')
param deployAlerts bool = false

@description('Tags with which to tag the resource in Azure.')
param tagValues object

var severityLevel = severityMapping[alertSeverity]

var alertFullDescription = 'Fires when ${testDescription} fails from ${minFailedLocationsToAlert} or more of ${length(testLocations)} test locations. URL tested: ${url}.  This endpoint depends on FUAPI, PostgreSQL and Azure AI Search - if the Public API app itself is healthy, check those next. For more info, review guidance on diagnosing availability test failures https://dfe-gov-uk.visualstudio.com.mcas.ms/s101-Explore-Education-Statistics/_wiki/wikis/s101-Explore-Education-Statistics.wiki/18320/Public-API-availability-alert-investigation'

resource availabilityTest 'Microsoft.Insights/webtests@2022-06-15' = {
  name: name
  location: location
  kind: 'standard'
  tags: union(tagValues, {
    'hidden-link:${appInsightsId}': 'Resource'
  })
  properties: {
    SyntheticMonitorId: name
    Name: name
    Description: testDescription
    Enabled: enabled
    Frequency: frequencyInSeconds
    Timeout: timeoutInSeconds
    Kind: 'standard'
    RetryEnabled: true
    Locations: [
      for testLocation in testLocations: {
        Id: testLocation
      }
    ]
    Request: {
      RequestUrl: url
      HttpVerb: 'GET'
      ParseDependentRequests: false
      FollowRedirects: true
      Headers: [
        {
          key: 'ees-request-source'
          value: 'Azure uptime monitoring'
        }
      ]
    }
    ValidationRules: {
      ExpectedHttpStatusCode: 200
      SSLCheck: true
      SSLCertRemainingLifetimeCheck: 7
      ContentValidation: {
        ContentMatch: contentMatch
        IgnoreCase: true
        PassIfTextFound: true
      }
    }
  }
}

resource alertsActionGroup 'Microsoft.Insights/actionGroups@2023-01-01' existing = if (deployAlerts) {
  name: alertsGroupName
}

resource availabilityAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = if (deployAlerts) {
  name: '${name}-availability'
  location: 'Global'
  properties: {
    enabled: enabled
    description: alertFullDescription
    severity: severityLevel
    evaluationFrequency: alertEvaluationFrequency
    windowSize: alertWindowSize
    scopes: [
      availabilityTest.id
      appInsightsId
    ]
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.WebtestLocationAvailabilityCriteria'
      webTestId: availabilityTest.id
      componentId: appInsightsId
      failedLocationCount: minFailedLocationsToAlert
    }
    actions: [
      {
        actionGroupId: alertsActionGroup.id
      }
    ]
  }
  tags: tagValues
}

output availabilityTestId string = availabilityTest.id
