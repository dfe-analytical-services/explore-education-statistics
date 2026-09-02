using '../main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101p02'
  environmentName: 'Pre-Production'
  domain: 'pre-production.explore-education-statistics.service.gov.uk'
  publicApiUrl: 'pp-api.education.gov.uk/statistics-preprod'
  publicApiDocsUrl: 'pp-api.education.gov.uk/statistics-preprod/docs'
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
    tier: 'Basic'
    name: 'B1'
  }
  googleAnalyticsTrackingId: 'G-8FSLWXTV2W'
}
