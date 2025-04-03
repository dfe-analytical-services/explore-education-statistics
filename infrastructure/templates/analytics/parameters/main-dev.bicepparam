using '../main.bicep'

// Environment Params
param environmentName = 'Development'

// On Dev, we will run the Analytic's function app's public API query consumer every 10 minutes.
param publicApiQueryConsumerCron = '0 */10 * * * *'

// On Dev, we will run the Analytic's function app's public zip downloads consumer every 10 minutes, starting at 5 mins past the hour.
param publicZipDownloadsConsumerCron = '0 5/10 * * * *'
