import {ContactDetails, IdLabelPair} from "@admin/services/common/types";
import {CreatePublicationRequest} from "@admin/services/edit-publication/types";
import {createClient} from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface PublicationService {
  getMethodologies: () => Promise<IdLabelPair[]>;
  getPublicationAndReleaseContacts: () => Promise<ContactDetails[]>;
  createPublication: (createRequest: CreatePublicationRequest) => Promise<void>;
}

const service: PublicationService = {
  getMethodologies(): Promise<IdLabelPair[]> {
    return apiClient.then(client =>
      client.get<IdLabelPair[]>(`/methodologies`),
    );
  },
  getPublicationAndReleaseContacts(): Promise<ContactDetails[]> {
    return apiClient.then(client =>
      client.get<ContactDetails[]>(`/publication/contacts`),
    );
  },
  createPublication(createRequest) {
    return apiClient.then(client =>
      client.post(`/publication/create`, createRequest),
    );
  }
};

export default service;
