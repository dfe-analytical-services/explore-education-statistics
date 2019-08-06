import { IdTitlePair } from '@admin/services/common/types';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface ReleaseSummaryService {
  getTemplateRelease: (
    publicationId: string,
  ) => Promise<IdTitlePair | undefined>;
  createRelease: (
    createRequest: CreateReleaseRequest,
  ) => Promise<ReleaseSummaryDetails>;
}

const service: ReleaseSummaryService = {
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
  ): Promise<ReleaseSummaryDetails> {
    return apiClient.then(client =>
      client.post(
        `/publications/${createRequest.publicationId}/releases`,
        createRequest,
      ),
    );
  },
};

export default service;
