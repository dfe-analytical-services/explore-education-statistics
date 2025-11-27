import { createQueryKeys } from '@lukemorales/query-key-factory';
import releaseContentService from '@admin/services/releaseContentService';

const releaseContentQueries = createQueryKeys('releaseContent', {
  get(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => releaseContentService.getContent(releaseId),
    };
  },
  getDataContent(releaseId: string) {
    return {
      queryKey: ['dataContent', releaseId],
      queryFn: () => releaseContentService.getDataContent(releaseId),
    };
  },
});

export default releaseContentQueries;
