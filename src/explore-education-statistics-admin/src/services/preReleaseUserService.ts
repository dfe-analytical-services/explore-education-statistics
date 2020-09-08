import client from '@admin/services/utils/service';

export interface PreReleaseUser {
  email: string;
  invited: boolean;
}

const preReleaseUserService = {
  getUsers(releaseId: string): Promise<PreReleaseUser[]> {
    return client.get<PreReleaseUser[]>(
      `/release/${releaseId}/prerelease-contacts`,
    );
  },
  inviteUser(releaseId: string, email: string): Promise<PreReleaseUser[]> {
    return client.post<PreReleaseUser[]>(
      `/release/${releaseId}/prerelease-contact`,
      {
        email,
      },
    );
  },
  removeUser(releaseId: string, email: string): Promise<PreReleaseUser[]> {
    return client.delete<PreReleaseUser[]>(
      `/release/${releaseId}/prerelease-contact`,
      {
        data: {
          email,
        },
      },
    );
  },
};

export default preReleaseUserService;
