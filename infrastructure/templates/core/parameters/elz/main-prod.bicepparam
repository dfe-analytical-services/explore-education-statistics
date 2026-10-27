// ELZ version of ../main-prod.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../main.bicep'
extends '../main-prod.bicepparam'

param elz = true

// TODO EES-7421 - placeholder ranges until the platform team allocates the ELZ ranges. Laid out as in "Subnet migration"
// in azure-resource-update-plan-for-elz.md: one integration subnet per App Service plan, sized to twice its maximum
// instance count (/26 for up to 15 instances, /27 for up to 5), and one subnet shared by all private endpoints.
param vNetAddressRanges = {
  vNet: '10.216.200.0/21'
  subnets: {
    // Shared infrastructure subnets.
    containerAppEnvironment: '10.216.200.0/24'
    privateEndpoints: '10.216.201.0/24'
    applicationGateway: '10.216.202.0/26'

    // App Service plan integration subnets.
    content: '10.216.202.64/26' // Content API, up to 15 instances
    data: '10.216.202.128/26' // Data API, up to 15 instances
    nlSearchFunctionApp: '10.216.202.192/26' // NL Search, up to 15 instances
    admin: '10.216.203.0/27'
    // 10.216.203.32/27 is reserved for the Public Site's integration subnet, once it has VNet integration.
    screenerFunctionApp: '10.216.203.64/27'
    publicApiDataProcessor: '10.216.203.96/27'
    analyticsFunctionApp: '10.216.203.128/27'
    searchDocsFunctionApp: '10.216.203.160/27'
    importer: '10.216.203.192/27'
    notify: '10.216.203.224/27'
    publisher: '10.216.204.0/27'

    // 10.216.204.32 - 10.216.207.255 is spare for growth.
  }
}
