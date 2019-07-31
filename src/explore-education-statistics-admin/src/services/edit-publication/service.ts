import {ContactDetails, IdLabelPair} from "@admin/services/common/types";
import {createClient} from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface PublicationService {
  getMethodologies: () => Promise<IdLabelPair[]>;
  getPublicationAndReleaseContacts: () => Promise<ContactDetails[]>;
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
};

export default service;
