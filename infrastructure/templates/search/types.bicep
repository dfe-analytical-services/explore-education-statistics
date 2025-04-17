@export()
type ResourceNames = {
  existingResources: {
    adminApp: string
    publisherFunction: string
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
@sealed()
type SearchStorageQueueNames = {
  @description('The name of the queue used when a publication is archived.')
  publicationArchivedQueueName: string
  @description('The name of the queue used when a publication is changed.')
  publicationChangedQueueName: string
  @description('The name of the queue used when the latest published release of a publication changes due to reordering.')
  publicationLatestPublishedReleaseReorderedQueueName: string
  @description('The name of the queue used when a searchable document requires a refresh.')
  refreshSearchableDocumentQueueName: string
  @description('The name of the queue used when a release slug is changed.')
  releaseSlugChangedQueueName: string
  @description('The name of the queue used when a release version is published.')
  releaseVersionPublishedQueueName: string
  @description('The name of the queue used for removing searchable documents associated with a publication.')
  removePublicationSearchableDocumentsQueueName: string
  @description('The name of the queue used when a searchable document is created.')
  searchableDocumentCreatedQueueName: string
  @description('The name of the queue used when a theme is updated.')
  themeUpdatedQueueName: string
}
