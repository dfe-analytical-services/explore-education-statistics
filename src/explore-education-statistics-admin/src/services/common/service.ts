import { IdLabelPair } from '@admin/services/common/types';
import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface CommonService {
  getPublicationDetailsForRelease(releaseId: string): Promise<IdLabelPair>;
}

const service: CommonService = {
  getPublicationDetailsForRelease(releaseId: string): Promise<IdLabelPair> {
    return apiClient.then(client =>
      client.get<IdLabelPair>(`/release/${releaseId}/publication`),
    );
  },
};

export default service;
