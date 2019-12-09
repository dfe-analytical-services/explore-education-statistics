import { IdTitlePair } from '@admin/services/common/types';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import client from '@admin/services/util/service';
import { Publication } from '@common/services/publicationService';

const service = {
  getTemplateRelease: (
    publicationId: string,
  ): Promise<IdTitlePair | undefined> => {
    return client
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
      );
  },
  getPublication(publicationId: string): Promise<Publication> {
    return client.get<Publication>(`publications/${publicationId}/`);
  },
  createRelease(
    createRequest: CreateReleaseRequest,
  ): Promise<ReleaseSummaryDetails> {
    return client.post(
      `/publications/${createRequest.publicationId}/releases`,
      createRequest,
    );
  },
};

export default service;
