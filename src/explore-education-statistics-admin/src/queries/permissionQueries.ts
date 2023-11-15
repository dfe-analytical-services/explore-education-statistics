import permissionService from '@admin/services/permissionService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const permissionQueries = createQueryKeys('releaseDataBlocks', {
  canUpdateRelease(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => permissionService.canUpdateRelease(releaseId),
    };
  },
  canUpdateMethodology(methodologyId: string) {
    return {
      queryKey: [methodologyId],
      queryFn: () => permissionService.canUpdateMethodology(methodologyId),
    };
  },
  getMethodologyApprovalPermissions(methodologyId: string) {
    return {
      queryKey: [methodologyId],
      queryFn: () =>
        permissionService.getMethodologyApprovalPermissions(methodologyId),
    };
  },
});

export default permissionQueries;
