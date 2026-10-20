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
  // Bursty: 45% of windows have 5xx, p95 161, p99 417. Kept sensitive while the bursts are investigated.
  publicSite: {
    threshold: 5
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
  // 93% of windows have no 5xx, p99 7, max 25.
  dataApi: {
    threshold: 10
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
  // 95% of windows have no 5xx, p99 8, with rare large bursts (max 1961).
  contentApi: {
    threshold: 10
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
  // 95% of windows have no 5xx, p99 15.
  admin: {
    threshold: 20
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
}

param http401Alerts = {
  // 98.5% of windows have no 401s, p99 3, max 165. Routine token expiry accounts for small counts.
  admin: {
    threshold: 10
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
}
