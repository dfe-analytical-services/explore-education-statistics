using '../main.bicep'

//Environment Params -------------------------------------------------------------------
param subscription = 's101t01'
param environmentName = 'Test'

//PostgreSQL Database Params -------------------------------------------------------------------
param dbAdminName = 'PostgreSQLAdmin'
param dbAdminPassword = 'adminPassword'
param dbSkuName = 'Standard_B1ms'
param dbStorageSizeGB = 32
param dbAutoGrowStatus = 'Disabled'

//Container App Params -------------------------------------------------------------------
param containerAppName = 'eesapi'
param containerAppImageName = 'azuredocs/aci-helloworld'
param containerAppTargetPort = 80
param useDummyImage = true
