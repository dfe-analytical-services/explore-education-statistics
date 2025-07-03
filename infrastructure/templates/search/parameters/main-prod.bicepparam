using '../main.bicep'

// Environment Params
param environmentName = 'Production'

param contentApiUrl = 'https://content.explore-education-statistics.service.gov.uk'
param searchServiceIndexerName = '' // Overridden by the Azure Pipeline
param searchServiceSemanticRankerAvailability = 'standard'
