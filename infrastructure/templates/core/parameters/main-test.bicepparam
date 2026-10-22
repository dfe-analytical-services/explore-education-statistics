using '../main.bicep'

// Environment Params
param environmentName = 'Test'

param publicSiteInternalServiceFqdn = 's101t01-ees-fde-dscafufydubae2fg.a02.azurefd.net'

param contentApiUrl = 'https://content.test.explore-education-statistics.service.gov.uk'
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

param vNetAddressRanges = {
  vNet: '10.0.0.0/16'
  subnets: {
    // Core shared infrastructure subnets.
    applicationGateway: '10.0.10.0/24'
    eventGridCustomTopicPrivateEndpoints: '10.0.20.0/24'

    // Core application subnets.
    admin: '10.0.0.0/24'
    importer: '10.0.1.0/24'
    publisher: '10.0.2.0/24'
    notify: '10.0.3.0/24'
    content: '10.0.4.0/24'
    data: '10.0.5.0/24'
    sqlServerPrivateEndpoints: '10.0.23.0/24'

    // Screener subnets.
    screenerFunctionApp: '10.0.19.0/24'
    screenerStoragePrivateEndpoints: '10.0.18.0/24'

    // Public API subnets.
    publicApiDataProcessor: '10.0.6.0/24'
    publicApiDataProcessorPrivateEndpoints: '10.0.7.0/24'
    publicApiStoragePrivateEndpoints: '10.0.11.0/24'
    containerAppEnvironment: '10.0.8.0/23'
    psqlFlexibleServer: '10.0.12.0/24'

    // NL Search subnets.
    nlSearchFunctionApp: '10.0.22.0/24'
    nlSearchFunctionAppPrivateEndpoints: '10.0.21.0/24'

    // Search subnets.
    searchDocsFunctionApp: '10.0.14.0/24'
    searchDocsFunctionAppPrivateEndpoints: '10.0.15.0/24'
    searchStoragePrivateEndpoints: '10.0.13.0/24'

    // Analytics subnets.
    analyticsFunctionApp: '10.0.17.0/24'
    analyticsStoragePrivateEndpoints: '10.0.16.0/24'
  }
}
