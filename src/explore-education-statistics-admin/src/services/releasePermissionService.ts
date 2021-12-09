import client from '@admin/services/utils/service';

export interface ManageAccessPageContributor {
  userId: string;
  userDisplayName: string;
  userEmail: string;
}

const releasePermissionService = {
  listReleaseContributors(
    releaseId: string,
  ): Promise<ManageAccessPageContributor[]> {
    return client.get(`/releases/${releaseId}/contributors`);
  },
  listPublicationContributors(
    publicationId: string,
  ): Promise<ManageAccessPageContributor[]> {
    return client.get(`/publications/${publicationId}/contributors`);
  },
  updateReleaseContributors(
    releaseId: string,
    userIds: string[],
  ): Promise<void> {
    return client.put(`/releases/${releaseId}/contributors`, { userIds });
  },
  removeAllUserContributorPermissionsForPublication(
    publicationId: string,
    userId: string,
  ): Promise<void> {
    return client.delete(
      `/publications/${publicationId}/users/${userId}/contributors`,
    );
  },
};

export default releasePermissionService;
