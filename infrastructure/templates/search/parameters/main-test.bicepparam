using '../main.bicep'

// Environment Params
param environmentName = 'Test'

param contentApiUrl = 'https://content.test.explore-education-statistics.service.gov.uk'
param searchServiceIndexerName = '' // Overridden by the Azure Pipeline
param searchServiceSemanticRankerAvailability = 'free'
