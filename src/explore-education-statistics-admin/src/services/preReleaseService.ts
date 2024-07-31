import client from '@admin/services/utils/service';

export interface PreReleaseSummary {
  contactEmail: string;
  contactTeam: string;
  publicationSlug: string;
  publicationTitle: string;
  releaseSlug: string;
  releaseTitle: string;
}

const preReleaseService = {
  getPreReleaseSummary(releaseVersionId: string): Promise<PreReleaseSummary> {
    return client.get<PreReleaseSummary>(
      `/release/${releaseVersionId}/prerelease`,
    );
  },
};

export default preReleaseService;
