import client from '@admin/services/utils/service';

export interface ContributorViewModel {
  userId: string;
  userDisplayName: string;
  userEmail: string;
}

export interface ContributorInvite {
  email: string;
}

const releasePermissionService = {
  listReleaseContributors(releaseId: string): Promise<ContributorViewModel[]> {
    return client.get(`/releases/${releaseId}/contributors`);
  },
  listReleaseContributorInvites(
    releaseId: string,
  ): Promise<ContributorInvite[]> {
    return client.get(`/releases/${releaseId}/contributor-invites`);
  },
  listPublicationContributors(
    publicationId: string,
  ): Promise<ContributorViewModel[]> {
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
