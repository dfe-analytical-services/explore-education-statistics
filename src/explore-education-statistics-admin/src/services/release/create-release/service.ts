import { IdTitlePair } from '@admin/services/common/types';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import client from '@admin/services/util/service';
import { Publication } from '@common/services/publicationService';

const service = {
  getTemplateRelease: (
    publicationId: string,
  ): Promise<IdTitlePair | undefined> => {
    return client.get(`/publications/${publicationId}/releases/template`);
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
  deleteRelease(releaseId: string): Promise<void> {
    return client.delete(`/release/${releaseId}`);
  },
  createReleaseAmendment(releaseId: string): Promise<ReleaseSummaryDetails> {
    return client.post(`/release/${releaseId}/amendment`);
  },
};

export default service;
