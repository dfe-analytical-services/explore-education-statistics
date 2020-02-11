import { ExternalMethodology } from '../dashboard/types';

export interface CreatePublicationRequest {
  topicId: string;
  publicationTitle: string;
  selectedMethodologyId?: string;
  selectedContactId: string;
  externalMethodology?: ExternalMethodology;
}

export default {};
