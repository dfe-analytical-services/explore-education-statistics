using '../main.bicep'

// Environment Params
param environmentName = 'Test'

param publicSiteInternalServiceFqdn = 's101t01-ees-fde-dscafufydubae2fg.a02.azurefd.net'

param contentApiUrl = 'https://content.test.explore-education-statistics.service.gov.uk'
param contentApiCertificateType = 'BringYourOwn'
param publicApiApplicationGatewayFqdn = 'test.statistics.api.education.gov.uk'
param publicApiPublicUrl = 'https://pp-api.education.gov.uk/statistics-test'
