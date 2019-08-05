import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import { ReleaseSetupDetails } from '@admin/services/release/types';
import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface ReleaseSetupService {
  createRelease: (
    createRequest: CreateReleaseRequest,
  ) => Promise<ReleaseSetupDetails>;
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
};

export default service;
