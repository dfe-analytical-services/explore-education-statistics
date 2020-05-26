import { UpdateReleaseSummaryDetailsRequest } from '@admin/services/release/edit-release/summary/types';
import {
  ReleasePublicationStatus,
  ReleaseSummaryDetails,
} from '@admin/services/release/types';
import client from '@admin/services/util/service';

const service = {
  async getReleaseSummaryDetails(
    releaseId: string,
  ): Promise<ReleaseSummaryDetails> {
    return client.get(`/releases/${releaseId}/summary`);
  },
  updateReleaseSummaryDetails(
    updateRequest: UpdateReleaseSummaryDetailsRequest,
  ): Promise<void> {
    return client.put(
      `/releases/${updateRequest.releaseId}/summary`,
      updateRequest,
    );
  },
  getReleasePublicationStatus(
    releaseId: string,
  ): Promise<ReleasePublicationStatus> {
    return client.get<ReleasePublicationStatus>(
      `/releases/${releaseId}/publication-status`,
    );
  },
};

export default service;
