import { createQueryKeys } from '@lukemorales/query-key-factory';
import dataReplacementService from '@admin/services/dataReplacementService';

const dataFileReplacementQueries = createQueryKeys('user', {
  getReplacementPlan(
    releaseVersionId: string,
    dataFileId: string,
    // We pass in the replacementDataFileId even though it isn't used in the request, as it is possible someone could
    // import a replacement, cancel it, and then import a new replacement, but as the dataFileId hasn't changed, could
    // receive a cached version of the replacement plan. Hence why we include replacementDataFileId in the queryKey
    replacementDataFileId: string,
  ) {
    return {
      queryKey: [dataFileId, replacementDataFileId],
      queryFn: () =>
        dataReplacementService.getReplacementPlan(releaseVersionId, dataFileId),
    };
  },
});

export default dataFileReplacementQueries;
