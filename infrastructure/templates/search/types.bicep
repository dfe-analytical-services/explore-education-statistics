@export()
type ResourceNames = {
  existingResources: {
    keyVault: string
    vNet: string
    alertsGroup: string
    subnets: {
      searchDocsFunction: string
      searchDocsFunctionPrivateEndpoints: string
      searchStoragePrivateEndpoints: string
    }
  }
}
