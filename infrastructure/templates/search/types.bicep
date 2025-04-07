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

@export()
// The built in Search Service roles. See https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles/ai-machine-learning
type SearchServiceRole = 'Search Index Data Contributor' | 'Search Index Data Reader' | 'Search Service Contributor'

@export()
type StorageQueueNamesType = {
  publicationChangedQueueName: string
  publicationLatestPublishedReleaseVersionChangedQueueName: string
  refreshSearchableDocumentQueueName: string
  releaseSlugChangedQueueName: string
  releaseVersionPublishedQueueName: string
  searchableDocumentCreatedQueueName: string
  themeUpdatedQueueName: string
}
