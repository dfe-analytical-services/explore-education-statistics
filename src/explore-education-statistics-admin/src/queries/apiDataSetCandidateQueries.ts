import apiDataSetCandidateService from '@admin/services/apiDataSetCandidateService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const apiDataSetCandidateQueries = createQueryKeys('apiDataSetCandidates', {
  list(releaseVersionId: string) {
    return {
      queryKey: [releaseVersionId],
      queryFn: () =>
        apiDataSetCandidateService.listCandidates(releaseVersionId),
    };
  },
});

export default apiDataSetCandidateQueries;
