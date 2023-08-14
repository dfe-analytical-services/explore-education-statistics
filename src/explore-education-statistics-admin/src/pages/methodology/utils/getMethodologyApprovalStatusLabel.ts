import { MethodologyApprovalStatus } from '@admin/services/methodologyService';

export default function getMethodologyApprovalStatusLabel(
  approvalStatus: MethodologyApprovalStatus,
): string {
  switch (approvalStatus) {
    case 'Draft':
      return 'Draft';
    case 'HigherLevelReview':
      return 'In Review';
    case 'Approved':
      return 'Approved';
    default:
      throw new Error(
        `Unsupported methodology approval status: ${approvalStatus}`,
      );
  }
}
