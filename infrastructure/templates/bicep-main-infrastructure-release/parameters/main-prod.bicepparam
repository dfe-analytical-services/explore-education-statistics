using '../main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101p01'
  environmentName: 'Production'
  domain: 'explore-education-statistics.service.gov.uk'
}

// Starter thresholds from 30 days of production App Insights request data (15-minute windows).
// Each fires on a Total strictly greater than the threshold. Services not listed (importer,
// publisher, notifier) keep the default Minimum > 0 rule.
param http5xxAlerts = {
  publicSite: {
    threshold: 10
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
  dataApi: {
    threshold: 10
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
  contentApi: {
    threshold: 10
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
  admin: {
    threshold: 10
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
}

param http401Alerts = {
  admin: {
    threshold: 10
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
  publicSite: {
    threshold: 10
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
}
