import permissionService from '@admin/services/permissionService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const permissionQueries = createQueryKeys('releaseDataBlocks', {
  canUpdateRelease(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => permissionService.canUpdateRelease(releaseId),
    };
  },
});

export default permissionQueries;
