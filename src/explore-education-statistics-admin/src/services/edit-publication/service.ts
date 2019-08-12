import { ContactDetails, IdTitlePair } from '@admin/services/common/types';
import { CreatePublicationRequest } from '@admin/services/edit-publication/types';
import client from '@admin/services/util/service';

export interface PublicationService {
  getMethodologies: () => Promise<IdTitlePair[]>;
  getPublicationAndReleaseContacts: () => Promise<ContactDetails[]>;
  createPublication: (createRequest: CreatePublicationRequest) => Promise<void>;
}

const service: PublicationService = {
  getMethodologies(): Promise<IdTitlePair[]> {
    return client.get<IdTitlePair[]>('/methodologies');
  },
  getPublicationAndReleaseContacts(): Promise<ContactDetails[]> {
    return client.get<ContactDetails[]>('/contacts');
  },
  createPublication(createRequest) {
    return client.post(`/topic/${createRequest.topicId}/publications`, {
      title: createRequest.publicationTitle,
      contactId: createRequest.selectedContactId,
      methodologyId: createRequest.selectedMethodologyId,
    });
  },
};

export default service;
