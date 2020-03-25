import { ExternalMethodology } from '../dashboard/types';

export interface PublicationMethodologyDetails {
  selectedMethodologyId?: string;
  externalMethodology?: ExternalMethodology;
}

export interface CreatePublicationRequest
  extends PublicationMethodologyDetails {
  topicId: string;
  publicationTitle: string;
  selectedContactId: string;
}

export default {};
