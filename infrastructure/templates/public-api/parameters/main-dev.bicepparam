using '../main.bicep'

// Environment Params
param subscription = 'dfe-devdw2'
param resourceGroupName = 'dfe-development-dw'
param environmentName = 'Development'

// PostgreSQL Database Params
param postgreSqlSkuName = 'Standard_B1ms'
param postgreSqlStorageSizeGB = 32
param postgreSqlAutoGrowStatus = 'Disabled'
