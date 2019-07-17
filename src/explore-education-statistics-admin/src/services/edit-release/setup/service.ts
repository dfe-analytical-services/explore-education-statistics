import {ReleaseSetupDetails, ReleaseSetupDetailsUpdateRequest,} from '@admin/services/edit-release/setup/types';
import {createClient} from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface ReleaseSetupService {
  getReleaseSetupDetails: (releaseId: string) => Promise<ReleaseSetupDetails>;
  updateReleaseSetupDetails: (updatedRelease: ReleaseSetupDetailsUpdateRequest) => Promise<void>;
}

const service: ReleaseSetupService = {
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

export default service;