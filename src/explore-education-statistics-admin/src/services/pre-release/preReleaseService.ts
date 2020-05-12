import client from '@admin/services/util/service';

export interface PreReleaseSummary {
  publicationTitle: string;
  releaseTitle: string;
}

const preReleaseService = {
  getPreReleaseSummary(releaseId: string): Promise<PreReleaseSummary> {
    return client.get<PreReleaseSummary>(`/release/${releaseId}/prerelease`);
  },
};

export default preReleaseService;
