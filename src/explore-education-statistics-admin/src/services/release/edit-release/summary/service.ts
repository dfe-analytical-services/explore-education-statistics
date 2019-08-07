import { ReleaseSummaryDetails } from '@admin/services/release/types';
import { UpdateReleaseSummaryDetailsRequest } from '@admin/services/release/edit-release/summary/types';
import client from '@admin/services/util/service';

export interface ReleaseSummaryService {
  getReleaseSummaryDetails: (
    releaseId: string,
  ) => Promise<ReleaseSummaryDetails>;
  updateReleaseSummaryDetails: (
    updatedRelease: UpdateReleaseSummaryDetailsRequest,
  ) => Promise<void>;
}

const service: ReleaseSummaryService = {
  getReleaseSummaryDetails(releaseId: string): Promise<ReleaseSummaryDetails> {
    return client.get<ReleaseSummaryDetails>(`/releases/${releaseId}/summary`);
  },
  updateReleaseSummaryDetails(
    updateRequest: UpdateReleaseSummaryDetailsRequest,
  ): Promise<void> {
    return client.post(
      `/releases/${updateRequest.releaseId}/summary`,
      updateRequest,
    );
  },
};

export default service;
