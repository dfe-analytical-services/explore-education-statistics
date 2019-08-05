import { ContactDetails, IdTitlePair } from '@admin/services/common/types';
import { CreatePublicationRequest } from '@admin/services/edit-publication/types';
import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface PublicationService {
  getMethodologies: () => Promise<IdTitlePair[]>;
  getPublicationAndReleaseContacts: () => Promise<ContactDetails[]>;
  createPublication: (createRequest: CreatePublicationRequest) => Promise<void>;
}

const service: PublicationService = {
  getMethodologies(): Promise<IdTitlePair[]> {
    return apiClient.then(client =>
      client.get<IdTitlePair[]>('/methodologies'),
    );
  },
  getPublicationAndReleaseContacts(): Promise<ContactDetails[]> {
    return apiClient.then(client => client.get<ContactDetails[]>('/contacts'));
  },
  createPublication(createRequest) {
    return apiClient.then(client =>
      client.post(`/topic/${createRequest.topicId}/publications`, {
        title: createRequest.publicationTitle,
        contactId: createRequest.selectedContactId,
        methodologyId: createRequest.selectedMethodologyId,
      }),
    );
  },
};

export default service;
