import client from '@admin/services/utils/service';

export interface ManageAccessPageContributor {
  userId: string;
  userFullName: string;
  userEmail: string;
  releaseRoleId: string | undefined;
}

const releasePermissionService = {
  getPublicationReleaseContributors(
    publicationId: string,
    releaseId: string,
  ): Promise<ManageAccessPageContributor[]> {
    return client.get(`/publications/${publicationId}/contributors`, {
      params: { releaseId },
    });
  },
  getAllPublicationContributors(
    releaseId: string,
  ): Promise<ManageAccessPageContributor[]> {
    return client.get(`/releases/${releaseId}/contributors`);
  },
  updateReleaseContributors(
    releaseId: string,
    userIds: string[],
  ): Promise<void> {
    return client.post(`/releases/${releaseId}/contributors`, userIds);
  },
  deleteAllUserContributorReleaseRolesForPublication(
    publicationId: string,
    userId: string,
  ): Promise<void> {
    return client.delete(
      `/publications/${publicationId}/users/${userId}/contributors`,
    );
  },
};

export default releasePermissionService;
