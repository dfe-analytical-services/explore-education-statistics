using '../main.bicep'

// Environment Params
param environmentName = 'Pre-Production'

param contentApiUrl = 'https://content.pre-production.explore-education-statistics.service.gov.uk'
param searchServiceIndexerName = '' // Overridden by the Azure Pipeline
param searchServiceSemanticRankerAvailability = 'free'
