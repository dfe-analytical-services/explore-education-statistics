import permissionService from '@admin/services/permissionService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const permissionQueries = createQueryKeys('releaseDataBlocks', {
  canUpdateRelease(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => permissionService.canUpdateRelease(releaseId),
    };
  },
  canUpdateMethodologyApprovalStatus(methodologyId: string) {
    return {
      queryKey: [methodologyId],
      queryFn: () =>
        permissionService.canUpdateMethodologyApprovalStatus(methodologyId),
    };
  },
});

export default permissionQueries;
