export interface CreatePublicationRequest {
  topicId: string,
  publicationTitle: string;
  selectedMethodologyId?: string;
  selectedContactId: string;
}