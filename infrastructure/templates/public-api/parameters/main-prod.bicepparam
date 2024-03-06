using '../main.bicep'

// Environment Params
param subscription = 's101p01'
param environmentName = 'Production'

// PostgreSQL Database Params
param postgreSqlSkuName = 'Standard_B1ms'
param postgreSqlStorageSizeGB = 32
param postgreSqlAutoGrowStatus = 'Disabled'

// Container App Params
param useDummyImage = true
