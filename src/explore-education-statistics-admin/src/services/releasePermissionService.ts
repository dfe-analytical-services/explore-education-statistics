import client from '@admin/services/utils/service';

export interface ReleaseContributorViewModel {
  userId: string;
  userFullName: string;
  releaseId: string;
  releaseRoleId: string;
}

const releasePermissionService = {
  getReleaseContributors(
    releaseId: string,
  ): Promise<ReleaseContributorViewModel[]> {
    return client.get(`/releases/${releaseId}/contributors`);
  },
};

export default releasePermissionService;
