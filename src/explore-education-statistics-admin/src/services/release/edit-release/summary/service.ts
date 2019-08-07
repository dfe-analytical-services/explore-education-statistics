import { ReleaseSummaryDetails } from '@admin/services/release/types';
import { UpdateReleaseSummaryDetailsRequest } from '@admin/services/release/edit-release/summary/types';
import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

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
    return apiClient.then(client =>
      client.get<ReleaseSummaryDetails>(`/releases/${releaseId}/summary`),
    );
  },
  updateReleaseSummaryDetails(
    updateRequest: UpdateReleaseSummaryDetailsRequest,
  ): Promise<void> {
    return apiClient.then(client =>
      client.post(
        `/releases/${updateRequest.releaseId}/summary`,
        updateRequest,
      ),
    );
  },
};

export default service;
