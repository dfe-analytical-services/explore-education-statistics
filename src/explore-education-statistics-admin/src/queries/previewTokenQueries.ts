import previewTokenService from '@admin/services/previewTokenService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const previewTokenQueries = createQueryKeys('previewTokenQueries', {
  list(dataSetVersionId: string) {
    return {
      queryKey: [dataSetVersionId],
      queryFn: () => previewTokenService.listPreviewTokens(dataSetVersionId),
    };
  },
  get(previewTokenId: string) {
    return {
      queryKey: [previewTokenId],
      queryFn: () => previewTokenService.getPreviewToken(previewTokenId),
    };
  },
});

export default previewTokenQueries;
