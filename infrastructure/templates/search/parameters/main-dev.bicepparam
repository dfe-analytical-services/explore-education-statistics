using '../main.bicep'

// Environment Params
param environmentName = 'Development'

param contentApiUrl = 'https://content.dev.explore-education-statistics.service.gov.uk'
param searchServiceIndexerName = '' // Overridden by the Azure Pipeline
param searchServiceSemanticRankerAvailability = 'free'
