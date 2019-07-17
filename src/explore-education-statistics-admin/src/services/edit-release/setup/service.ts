import { createClient } from '@admin/services/common/service';
import {
  ReleaseSetupDetails,
  ReleaseSetupDetailsUpdateRequest,
} from '@admin/services/edit-release/setup/types';
import mocks from './mock/axios-mock';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export default {
  getReleaseSetupDetails(releaseId: string): Promise<ReleaseSetupDetails> {
    return apiClient.then(client =>
      client.get<ReleaseSetupDetails>(`/release/${releaseId}/setup`),
    );
  },
  updateReleaseSetupDetails(
    updatedRelease: ReleaseSetupDetailsUpdateRequest,
  ): Promise<void> {
    return apiClient.then(client =>
      client.post(`/release/${updatedRelease.id}/setup`, updatedRelease),
    );
  },
};
