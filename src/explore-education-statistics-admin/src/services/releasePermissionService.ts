import client from '@admin/services/utils/service';

export interface ReleaseContributor {
  userId: string;
  userFullName: string;
  releaseId: string;
  releaseRoleId?: string;
}

const releasePermissionService = {
  getReleaseContributors(releaseId: string): Promise<ReleaseContributor[]> {
    return client.get(`/releases/${releaseId}/contributors`);
  },
};

export default releasePermissionService;
