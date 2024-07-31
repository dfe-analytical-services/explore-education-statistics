import client from '@admin/services/utils/service';

export interface PreReleaseUser {
  email: string;
}

export interface PreReleaseInvitePlan {
  alreadyAccepted: string[];
  alreadyInvited: string[];
  invitable: string[];
}

const preReleaseUserService = {
  getUsers(releaseVersionId: string): Promise<PreReleaseUser[]> {
    return client.get(`/release/${releaseVersionId}/prerelease-users`);
  },
  getInvitePlan(
    releaseVersionId: string,
    emails: string[],
  ): Promise<PreReleaseInvitePlan> {
    return client.post(`/release/${releaseVersionId}/prerelease-users-plan`, {
      emails,
    });
  },
  inviteUsers(
    releaseVersionId: string,
    emails: string[],
  ): Promise<PreReleaseUser[]> {
    return client.post(`/release/${releaseVersionId}/prerelease-users`, {
      emails,
    });
  },
  removeUser(releaseVersionId: string, email: string): Promise<void> {
    return client.delete(`/release/${releaseVersionId}/prerelease-users`, {
      data: {
        email,
      },
    });
  },
};

export default preReleaseUserService;
