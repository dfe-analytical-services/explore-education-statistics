@export()
type ResourceNames = {
  existingResources: {
    keyVault: string
    vNet: string
    alertsGroup: string
    logAnalyticsWorkspace: string
    subnets: {
      analyticsFunctionApp: string
      storagePrivateEndpoints: string
    }
  }
}
