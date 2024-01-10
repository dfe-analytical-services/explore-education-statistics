using './main.bicep'

//Environment Params -------------------------------------------------------------------
@description('Base domain name for Public API')
param domain = 'publicapi'

@description('Data Hub Subscription Name e.g. s101d01. Used as a prefix for created resources')
param subscription = 's101d01'

@description('Data Hub Environment Name e.g. api. Used as a prefix for created resources')
param environment = 'api'

//Tagging Params ------------------------------------------------------------------------
@description('Tag Value - Enter the Department name tag value e.g. Data Directorate')
param departmentName = 'Public API'
@description('Tag Value - The name of the phase of the development lifecycle environment that the component will be used in e.g. Development / Test / Integration / Production')
param environmentName = 'Development'
@description('Tag Value - Enter the solution name that the component is a part of e.g. EDAP, LDS, EES, API')
param solutionName = 'API'
@description('Tag Value - Enter the full name of the Azure subscription where this resource is located e.g. s101-datahub-development / s101-datahub-test / s101-datahub-production')
param subscriptionName = 'Unknown'
@description('Tag Value - Enter the cost centre identifying value provided by the Service Owner. Otherwise populate with Unknown.')
param costCentre = 'Unknown'
@description('Tag Value - Enter the name of the Service or Application Owner in the SURNAME, Firstname format e.g. SINCLAIR, Paul / SHELBY, Laura')
param serviceOwnerName = 'Unknown'
@description('Tag Value - Enter the name of the user who created these resources in the SURNAME, Firstname format e.g. FISHER, Paul')
param createdBy = 'Unknown'
@description('Tag Value - Enter the name of the repo that the deployment script for the component name be found. If the component is deployed manually, the value should be N/A')
param deploymentRepo = 'N/A'

//Networking Params --------------------------------------------------------------------------
@description('Networking : Deploy subnets for networking')
param deploySubnets = true

@description('Networking : Included whitelist')
param storageFirewallRules = ['0.0.0.0/0']

//Storage Params ------------------------------------------------------------------------------
@description('Storage : Name of the root fileshare folder name')
@minLength(3)
@maxLength(63)
param fileShareName = 'data'

@description('Storage : Type of the file share quota')
param fileShareQuota = 1

//PostgreSQL Database Params -------------------------------------------------------------------
@description('Database : administrator login name')
@minLength(1)
param dbAdministratorLoginName = 'PostgreSQLAdmin'

@description('Database : administrator password')
@minLength(8)
@secure()
param dbAdministratorLoginPassword = ''

@description('Database : Azure Database for PostgreSQL sku name ')
@allowed([
  'Standard_B1ms'
  'Standard_D4ads_v5'
  'Standard_E4ads_v5'
])
param skuName = 'Standard_B1ms'

@description('Database : Azure Database for PostgreSQL Storage Size in GB')
param storageSizeGB = 32

@description('Database : Azure Database for PostgreSQL Autogrow setting')
param autoGrowStatus = 'Disabled'

//Container Registry Params ----------------------------------------------------------------
@minLength(5)
@maxLength(50)
@description('Registry : Name of the azure container registry (must be globally unique)')
param containerRegistryName = 'eesapiacr'

//Container App Params
@description('Container App : Specifies the container image to deploy from the registry <name>:<tag>.')
param acrHostedImageName = 'hello-world'

@description('Specifies the container port.')
param targetPort = 80

@description('Container App : Specifies the container image to seed to the ACR.')
param containerSeedImage = 'mcr.microsoft.com/azuredocs/aci-helloworld'

@description('Select if you want to seed the ACR with a base image.')
param seedRegistry = true

//ServiceBus Queue Params -------------------------------------------------------------------
@description('Name of the Service Bus namespace')
param namespaceName = 'etlnamespace'

@description('Name of the Queue')
param queueName = 'etlfunctionqueue'

//ETL Function Paramenters ------------------------------------------------------------------
@description('Application : Insights name')
param applicationInsightsName = 'etlFunctionInsights'

@description('Function App Plan operating system')
@allowed([
  'Windows'
  'Linux'
])
param appServiceplanOS = 'Linux'

@description('Function App : Runtime Language')
@allowed([
  'dotnet'
  'node'
  'python'
  'java'
])
param functionAppRuntime = 'python'

