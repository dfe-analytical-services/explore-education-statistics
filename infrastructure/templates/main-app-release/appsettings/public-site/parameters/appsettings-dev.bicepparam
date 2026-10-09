using '../public-site-appsettings.bicep'

param environmentIdentifier = 's101d01'
param environmentName = 'Development'
param domain = 'dev.explore-education-statistics.service.gov.uk'
param basicAuthEnabled = true
param googleAnalyticsTrackingId = 'G-GRPHH2FN0L'
param defaultCacheMaxAgeSeconds = 10
param publicApiUrl = 'pp-api.education.gov.uk/statistics-dev'
