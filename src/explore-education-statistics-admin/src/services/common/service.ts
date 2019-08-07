import { IdTitlePair } from '@admin/services/common/types';
import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface CommonService {
  getPublicationDetailsForRelease(releaseId: string): Promise<IdTitlePair>;
  getReleaseTypes(): Promise<IdTitlePair[]>;
}

const service: CommonService = {
  getPublicationDetailsForRelease(releaseId: string): Promise<IdTitlePair> {
    return apiClient.then(client =>
      client.get<IdTitlePair>(`/release/${releaseId}/publication`),
    );
  },
  getReleaseTypes(): Promise<IdTitlePair[]> {
    return apiClient.then(client =>
      client.get<IdTitlePair[]>('/meta/releasetypes'),
    );
  },
};

export default service;
