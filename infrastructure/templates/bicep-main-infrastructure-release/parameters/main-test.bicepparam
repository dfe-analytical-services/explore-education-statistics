using '../main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101t01'
  environmentName: 'Test'
  domain: 'test.explore-education-statistics.service.gov.uk'
  detailedErrors: true
  autoscaleAppServices: false
}

param adminConfigParam = {
  appServiceSku: {
    tier: 'Standard'
    name: 'S1'
  }
  signalRSku: {
    name: 'Free_F1'
    capacity: 1
  }
}

param contentApiConfigParam = {
  appServiceSku: {
    tier: 'Standard'
    name: 'S1'
  }
}

param dataApiConfigParam = {
  appServiceSku: {
    tier: 'Standard'
    name: 'S1'
  }
}

param importerConfigParam = {
  appServiceSku: {
    tier: 'PremiumV2'
    name: 'P1V2'
  }
}

param notifierConfigParam = {
  appServiceSku: {
    tier: 'Basic'
    name: 'B1'
  }
}

param publisherConfigParam = {
  appServiceSku: {
    tier: 'Basic'
    name: 'B1'
  }
}

param publicSiteConfigParam = {
  appServiceSku: {
    tier: 'Basic'
    name: 'B1'
  }
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
}
