using '../main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101p01'
  environmentName: 'Production'
  domain: 'explore-education-statistics.service.gov.uk'
}

param publisherConfigParam = {
  frontDoorCachePurgeEnabled: true
}

param publicApiConfigParam = {
  publicUrl: 'api.education.gov.uk/statistics'
}

param publicSiteConfigParam = {
  googleAnalyticsTrackingId: 'G-9YG8ESXR5Y'
}
