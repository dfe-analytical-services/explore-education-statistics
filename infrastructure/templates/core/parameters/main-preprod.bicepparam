using '../main.bicep'

// Environment Params
param environmentName = 'Pre-Production'

param publicSiteInternalServiceFqdn = 's101p02-ees-fde-euhyh8d6cdeagqdu.a02.azurefd.net'

param publicApiApplicationGatewayFqdn = 'pre-production.statistics.api.education.gov.uk'
param publicApiPublicUrl = 'https://pp-api.education.gov.uk/statistics-preprod'

param recoveryServicesVaultImmutable = true

param contentDbConfig = {
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 50
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
  licenseType: 'BasePrice'
  maxSizeBytes: 1099511627776
}
