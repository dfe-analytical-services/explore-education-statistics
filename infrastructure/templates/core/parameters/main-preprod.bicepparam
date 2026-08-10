using '../main.bicep'

// Environment Params
param environmentName = 'Pre-Production'

param publicSiteInternalServiceFqdn = 's101p02-ees-fde-euhyh8d6cdeagqdu.a02.azurefd.net'

param contentApiUrl = 'https://cont.pre-production.explore-education-statistics.service.gov.uk'
param contentApiCertificateType = 'BringYourOwn'
param publicApiApplicationGatewayFqdn = 'pre-production.statistics.api.education.gov.uk'
param publicApiPublicUrl = 'https://pp-api.education.gov.uk/statistics-preprod'
