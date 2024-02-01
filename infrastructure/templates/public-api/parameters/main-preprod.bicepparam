using '../main.bicep'

//Environment Params -------------------------------------------------------------------
param domain = 'publicapi'
param subscription = 's101d01'
param environment = 'eesapi'
param environmentName = 'Pre-Prod'

//Networking Params --------------------------------------------------------------------------
param deploySubnets = true

//PostgreSQL Database Params -------------------------------------------------------------------
param dbAdminName = 'PostgreSQLAdmin'
param dbAdminPassword = 'adminPassword'
param dbSkuName = 'Standard_B1ms'
param dbStorageSizeGB = 32
param dbAutoGrowStatus = 'Disabled'

//Container Registry -------------------------------------------------------------------
param containerRegistryName = 'eesapiacr'
param deployRegistry = true

//Container App Params -------------------------------------------------------------------
param containerAppName = 'eesapi'
param containerAppImageName = 'azuredocs/aci-helloworld'
param containerAppTargetPort = 80
param useDummyImage = true
