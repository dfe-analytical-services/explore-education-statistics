using '../main.bicep'

// Environment Params
param subscription = 's101t01'
param environmentName = 'Test'

// PostgreSQL Database Params
param postgreSqlAdminName = 'PostgreSQLAdmin'
param postgreSqlAdminPassword = 'adminPassword'
param postgreSqlSkuName = 'Standard_B1ms'
param postgreSqlStorageSizeGB = 32
param postgreSqlAutoGrowStatus = 'Disabled'

// Container App Params
param useDummyImage = true
