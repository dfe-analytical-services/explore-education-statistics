using '../main.bicep'

// Environment Params
param environmentName = 'Production'

param averagePublicSiteResponseTimeAlertThresholdMillis = 1000

param publicSiteInternalServiceFqdn = 's101p01-ees-fde-hzgvd4b5effuaua2.a02.azurefd.net'

param publicApiApplicationGatewayFqdn = 'statistics.api.education.gov.uk'
param publicApiPublicUrl = 'https://api.education.gov.uk/statistics'
