export interface CreatePublicationRequest {
  topicId: string;
  publicationTitle: string;
  selectedMethodologyId?: string;
  selectedContactId: string;
}

export interface Topic {
  id: string;
  title: string;
  slug: string;
  description: string;
}

export default {};
