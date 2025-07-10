@export()
type ResourceNames = {
  existingResources: {
    keyVault: string
    logAnalyticsWorkspace: string
    vNet: string
    adminApp: string
    alertsGroup: string
    subnets: {
      screenerFunction: string
      screenerFunctionPrivateEndpoints: string
      adminApp: string
    }
    coreStorageAccount: string
  }
  screener: {
    screenerFunction: string
    screenerFunctionStorageAccount: string
  }
}
