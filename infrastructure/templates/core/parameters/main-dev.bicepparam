using '../main.bicep'

// Environment Params
param environmentName = 'Development'

param publicSiteInternalServiceFqdn = 's101d01-ees-fde-eve8c8hmd6gxgqcr.a03.azurefd.net'

param contentApiUrl = 'https://content.dev.explore-education-statistics.service.gov.uk'
param contentApiCertificateType = 'Provisioned'
param publicApiApplicationGatewayFqdn = 'dev.statistics.api.education.gov.uk'
param publicApiPublicUrl = 'https://pp-api.education.gov.uk/statistics-dev'
