import { UpdateReleaseStatusRequest } from '@admin/services/release/edit-release/status/types';
import releaseSummaryService from '@admin/services/release/edit-release/summary/service';
import client from '@admin/services/util/service';

const service = {
  getReleaseStatus: (releaseId: string) =>
    releaseSummaryService
      .getReleaseSummaryDetails(releaseId)
      .then(release => release.status),
  updateReleaseStatus: (
    releaseId: string,
    updateRequest: UpdateReleaseStatusRequest,
  ) => client.put(`/releases/${releaseId}/status`, updateRequest),
};

export default service;
