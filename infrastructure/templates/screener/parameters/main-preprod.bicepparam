using '../main.bicep'

// Environment Params
param environmentName = 'Pre-Production'

// Allow screening results to be logged in the Screener API log stream on Pre-Prod.
param logScreeningResults = true
