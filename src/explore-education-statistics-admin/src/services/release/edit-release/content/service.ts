import { ReleaseSummaryDetails } from '@admin/services/release/types';
import { UpdateReleaseSummaryDetailsRequest } from '@admin/services/release/edit-release/summary/types';
import client from '@admin/services/util/service';
import { Release } from '@common/services/publicationService';

export interface ReleaseContentService {
  getRelease: (
    releaseId: string,
  ) => Promise<Release>;
}

const service: ReleaseContentService = {
    getRelease(releaseId: string): Promise<Release> {
    return client.get<Release>(`/releases/${releaseId}`);
  },
};

export default service;
