import client from '@admin/services/utils/service';

export interface ApiDataSetCandidate {
  releaseFileId: string;
  title: string;
}

const apiDataSetCandidateService = {
  listCandidates(releaseVersionId: string): Promise<ApiDataSetCandidate[]> {
    return client.get(`/public-data/data-set-candidates`, {
      params: {
        releaseVersionId,
      },
    });
  },
};

export default apiDataSetCandidateService;
