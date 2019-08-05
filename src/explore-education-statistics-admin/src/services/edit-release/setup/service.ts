import {
  CreateReleaseRequest,
  ReleaseSetupDetails,
  UpdateReleaseSetupDetailsRequest,
} from '@admin/services/edit-release/setup/types';
import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface ReleaseSetupService {
  createRelease: (
    createRequest: CreateReleaseRequest,
  ) => Promise<ReleaseSetupDetails>;
  getReleaseSetupDetails: (releaseId: string) => Promise<ReleaseSetupDetails>;
  updateReleaseSetupDetails: (
    updatedRelease: UpdateReleaseSetupDetailsRequest,
  ) => Promise<void>;
}

const service: ReleaseSetupService = {
  createRelease(
    createRequest: CreateReleaseRequest,
  ): Promise<ReleaseSetupDetails> {
    return apiClient.then(client =>
      client.post(
        `/publication/${createRequest.publicationId}/releases`,
        createRequest,
      ),
    );
  },
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
