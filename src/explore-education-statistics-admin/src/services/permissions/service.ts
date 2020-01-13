import client from '@admin/services/util/service';

const service = {
  canUpdateRelease: (releaseId: string): Promise<boolean> => {
    return client.get(`/permissions/release/${releaseId}/update`);
  },
  canSubmitReleaseForHigherLevelReview: (
    releaseId: string,
  ): Promise<boolean> => {
    return client.get(`/permissions/release/${releaseId}/status/submit`);
  },
  canApproveRelease: (releaseId: string): Promise<boolean> => {
    return client.get(`/permissions/release/${releaseId}/status/approve`);
  },
};

export default service;
