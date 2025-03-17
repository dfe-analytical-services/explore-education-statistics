@export()
type ResourceNames = {
  existingResources: {
    keyVault: string
    vNet: string
    alertsGroup: string
    subnets: {
      analyticsFunctionApp: string
      storagePrivateEndpoints: string
    }
  }
}
