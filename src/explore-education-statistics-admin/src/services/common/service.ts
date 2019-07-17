import {IdLabelPair} from "@admin/services/common/types";
import {createClient} from '@admin/services/util/service';
import mocks from './mock/axios-mock';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export default {
  getPublicationDetailsForRelease(releaseId: string): Promise<IdLabelPair> {
    return apiClient.then(client =>
      client.get<IdLabelPair>(`/release/${releaseId}/publication`),
    );
  },
};
