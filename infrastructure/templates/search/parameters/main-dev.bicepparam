using '../main.bicep'

// Environment Params
param environmentName = 'Development'

param contentApiUrl = 'https://content.dev.explore-education-statistics.service.gov.uk'
param searchServiceSemanticRankerAvailability = 'free'

// Allow API key authentication for the Azure AI Search service to support local development.
param searchServiceLocalAuthEnabled = true

param deployNaturalLanguageSearchFunctionApp = true
