@export()
type ResourceNames = {
  existingResources: {
    keyVault: string
    vNet: string
    alertsGroup: string
    subnets: {
      searchStoragePrivateEndpoints: string
    }
  }
}
