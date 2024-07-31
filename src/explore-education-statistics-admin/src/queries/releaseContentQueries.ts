import { createQueryKeys } from '@lukemorales/query-key-factory';
import releaseContentService from '@admin/services/releaseContentService';

const releaseContentQueries = createQueryKeys('releaseContent', {
  get(releaseVersionId: string) {
    return {
      queryKey: [releaseVersionId],
      queryFn: () => releaseContentService.getContent(releaseVersionId),
    };
  },
});

export default releaseContentQueries;
