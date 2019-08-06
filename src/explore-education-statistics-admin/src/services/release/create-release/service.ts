import { IdTitlePair } from '@admin/services/common/types';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import { ReleaseSetupDetails } from '@admin/services/release/types';
import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface ReleaseSetupService {
  getTemplateRelease: (
    publicationId: string,
  ) => Promise<IdTitlePair | undefined>;
  createRelease: (
    createRequest: CreateReleaseRequest,
  ) => Promise<ReleaseSetupDetails>;
}

const service: ReleaseSetupService = {
  getTemplateRelease: (
    publicationId: string,
  ): Promise<IdTitlePair | undefined> => {
    return apiClient.then(client =>
      client
        .get(`/publications/${publicationId}/releases`)
        .then(
          results =>
            results as {
              id: string;
              title: string;
              latestRelease: boolean;
            }[],
        )
        .then(releases => releases.find(release => release.latestRelease))
        .then(
          release =>
            release && {
              id: release.id,
              title: release.title,
            },
        ),
    );
  },
  createRelease(
    createRequest: CreateReleaseRequest,
  ): Promise<ReleaseSetupDetails> {
    return apiClient.then(client =>
      client.post(
        `/publications/${createRequest.publicationId}/releases`,
        createRequest,
      ),
    );
  },
};

export default service;
