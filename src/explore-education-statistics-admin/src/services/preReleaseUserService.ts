import client from '@admin/services/utils/service';

export interface PreReleaseUser {
  email: string;
}

const preReleaseUserService = {
  getUsers(releaseId: string): Promise<PreReleaseUser[]> {
    return client.get(`/release/${releaseId}/prerelease-users`);
  },
  inviteUser(releaseId: string, email: string): Promise<PreReleaseUser> {
    return client.post(`/release/${releaseId}/prerelease-users`, {
      email,
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
