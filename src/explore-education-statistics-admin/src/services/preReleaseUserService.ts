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
  getUsers(releaseId: string): Promise<PreReleaseUser[]> {
    return client.get(`/release/${releaseId}/prerelease-users`);
  },
  getInvitePlan(
    releaseId: string,
    emails: string[],
  ): Promise<PreReleaseInvitePlan> {
    return client.post(`/release/${releaseId}/prerelease-users-plan`, {
      emails,
    });
  },
  inviteUsers(releaseId: string, emails: string[]): Promise<PreReleaseUser[]> {
    return client.post(`/release/${releaseId}/prerelease-users`, {
      emails,
    });
  },
  removeUser(releaseId: string, email: string): Promise<void> {
    return client.delete(`/release/${releaseId}/prerelease-users`, {
      data: {
        email,
      },
    });
  },
};

export default preReleaseUserService;
