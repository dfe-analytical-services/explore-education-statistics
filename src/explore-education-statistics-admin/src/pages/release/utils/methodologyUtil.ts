import { MethodologyApprovalStatus } from '@admin/services/methodologyService';

const getMethodologyApprovalStatusLabel = (
  approvalStatus: MethodologyApprovalStatus,
): string | undefined => {
  switch (approvalStatus) {
    case 'Draft':
      return 'Draft';
    case 'HigherLevelReview':
      return 'In Review';
    case 'Approved':
      return 'Approved';
    default:
      return undefined;
  }
};

export default getMethodologyApprovalStatusLabel;
