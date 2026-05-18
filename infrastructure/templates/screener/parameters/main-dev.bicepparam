using '../main.bicep'

// Environment Params
param environmentName = 'Development'

// Allow screening results to be logged in the Screener API log stream on Dev.
param logScreeningResults = true