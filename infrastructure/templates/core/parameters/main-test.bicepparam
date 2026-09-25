using '../main.bicep'

// Environment Params
param environmentName = 'Test'

param publicSiteInternalServiceFqdn = 's101t01-ees-fde-dscafufydubae2fg.a02.azurefd.net'

param publicApiApplicationGatewayFqdn = 'test.statistics.api.education.gov.uk'
param publicApiPublicUrl = 'https://pp-api.education.gov.uk/statistics-test'

param sqlServerPublicNetworkAccess = 'Disabled'

param contentDbConfig = {
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 10
  }
  licenseType: 'LicenseIncluded'
  maxSizeBytes: 1073741824
}

param statisticsDbConfig = {
  sku: {
    name: 'GP_Gen5'
    tier: 'GeneralPurpose'
    capacity: 2
  }
  licenseType: 'LicenseIncluded'
  maxSizeBytes: 375809638400
}
