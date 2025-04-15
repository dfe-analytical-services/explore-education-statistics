using '../main.bicep'

// Environment Params
param environmentName = 'Development'

// On Dev, we will run the Function App every 10 minutes.
param analyticsRequestFilesConsumerCron = '0 */10 * * * *'
