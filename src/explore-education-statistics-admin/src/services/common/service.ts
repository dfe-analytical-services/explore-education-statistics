import {BasicPublicationDetails, IdTitlePair} from '@admin/services/common/types';
import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface CommonService {
  getBasicPublicationDetails(publicationId: string): Promise<BasicPublicationDetails>;
  getReleaseTypes(): Promise<IdTitlePair[]>;
}

const service: CommonService = {
  getBasicPublicationDetails(publicationId: string): Promise<BasicPublicationDetails> {
    return apiClient.then(client =>
      client.get<BasicPublicationDetails>(`/publications/${publicationId}`),
    );
  },
  getReleaseTypes(): Promise<IdTitlePair[]> {
    return apiClient.then(client =>
      client.get<IdTitlePair[]>('/meta/releasetypes'),
    );
  },
};

export default service;
