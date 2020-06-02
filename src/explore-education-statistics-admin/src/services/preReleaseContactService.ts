import client from '@admin/services/utils/service';

export interface PrereleaseContactDetails {
  email: string;
  invited: boolean;
}

const preReleaseContactService = {
  getPreReleaseContactsForRelease(
    releaseId: string,
  ): Promise<PrereleaseContactDetails[]> {
    return client.get<PrereleaseContactDetails[]>(
      `/release/${releaseId}/prerelease-contacts`,
    );
  },
  addPreReleaseContactToRelease(
    releaseId: string,
    email: string,
  ): Promise<PrereleaseContactDetails[]> {
    return client.post<PrereleaseContactDetails[]>(
      `/release/${releaseId}/prerelease-contact`,
      {
        email,
      },
    );
  },
  removePreReleaseContactFromRelease(
    releaseId: string,
    email: string,
  ): Promise<PrereleaseContactDetails[]> {
    return client.delete<PrereleaseContactDetails[]>(
      `/release/${releaseId}/prerelease-contact`,
      {
        data: {
          email,
        },
      },
    );
  },
};

export default preReleaseContactService;
