using '../main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101p02'
  environmentName: 'Pre-Production'
  domain: 'pre-production.explore-education-statistics.service.gov.uk'
}

param importerConfigParam = {
  appServiceSku: {
    tier: 'PremiumV2'
    name: 'P1V2'
  }
}

param publicSiteConfigParam = {
  appServiceSku: {
    tier: 'PremiumV2'
    name: 'P1V2'
  }
}
