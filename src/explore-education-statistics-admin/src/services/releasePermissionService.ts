import client from '@admin/services/utils/service';

export interface UserReleaseRole {
  userId: string;
  userDisplayName: string;
  userEmail: string;
  role: 'Contributor' | 'Approver';
}

export interface UserReleaseInvite {
  email: string;
  role: 'Contributor' | 'Approver';
}

const releasePermissionService = {
  listRoles(releaseId: string): Promise<UserReleaseRole[]> {
    return client.get(`/releases/${releaseId}/roles`);
  },
  listInvites(releaseId: string): Promise<UserReleaseInvite[]> {
    return client.get(`/releases/${releaseId}/invites`);
  },
  listPublicationContributors(
    publicationId: string,
  ): Promise<UserReleaseRole[]> {
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
