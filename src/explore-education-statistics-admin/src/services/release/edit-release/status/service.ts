import { UpdateReleaseStatusRequest } from '@admin/services/release/edit-release/status/types';
import releaseSummaryService from '@admin/services/release/edit-release/summary/service';
import client from '@admin/services/util/service';

export interface ReleaseStatusService {
  getReleaseStatus: (releaseId: string) => Promise<string>;
  updateReleaseStatus: (
    releaseId: string,
    values: UpdateReleaseStatusRequest,
  ) => Promise<void>;
}

const service: ReleaseStatusService = {
  getReleaseStatus: releaseId =>
    releaseSummaryService
      .getReleaseSummaryDetails(releaseId)
      .then(release => release.status),
  updateReleaseStatus: (releaseId, updateRequest) =>
    client.put(`/releases/${releaseId}/status`, updateRequest),
};

export default service;
