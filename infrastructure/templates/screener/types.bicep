@export()
type ResourceNames = {
  existingResources: {
    keyVault: string
    vNet: string
    adminApp: string
    alertsGroup: string
    subnets: {
      screenerFunction: string
      screenerFunctionPrivateEndpoints: string
      adminApp: string
    }
  }
  screener: {
    screenerFunction: string
    screenerFunctionStorageAccount: string
    screenerFunctionIdentity: string
  }
}
