using '../main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101p02'
  environmentName: 'Pre-Production'
  domain: 'pre-production.explore-education-statistics.service.gov.uk'
  basicAuthEnabled: true
}

param adminConfigParam = {
  enableThemeDeletion: true
}

param publicApiConfigParam = {
  publicUrl: 'pp-api.education.gov.uk/statistics-preprod'
}

param publicSiteConfigParam = {
  appServiceSku: {
    tier: 'PremiumV2'
    name: 'P1V2'
  }
  googleAnalyticsTrackingId: 'G-8FSLWXTV2W'
}
