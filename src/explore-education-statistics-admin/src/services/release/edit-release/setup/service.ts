import {ReleaseSetupDetails} from '@admin/services/release//types';
import {UpdateReleaseSetupDetailsRequest} from "@admin/services/release/edit-release/setup/types";
import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface ReleaseSetupService {
  getReleaseSetupDetails: (releaseId: string) => Promise<ReleaseSetupDetails>;
  updateReleaseSetupDetails: (
    updatedRelease: UpdateReleaseSetupDetailsRequest,
  ) => Promise<void>;
}

const service: ReleaseSetupService = {
  getReleaseSetupDetails(releaseId: string): Promise<ReleaseSetupDetails> {
    return apiClient.then(client =>
      client.get<ReleaseSetupDetails>(`/release/${releaseId}/setup`),
    );
  },
  updateReleaseSetupDetails(
    updateRequest: UpdateReleaseSetupDetailsRequest,
  ): Promise<void> {
    return apiClient.then(client =>
      client.post(`/release/${updateRequest.releaseId}/setup`, updateRequest),
    );
  },
};

export default service;
