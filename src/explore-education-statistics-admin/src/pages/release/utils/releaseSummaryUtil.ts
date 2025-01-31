import { Release } from '@admin/services/releaseService';
import { ReleaseApprovalStatus } from '@common/services/publicationService';

export const getReleaseApprovalStatusLabel = (
  approvalStatus: ReleaseApprovalStatus,
) => {
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

const getLiveLatestLabel = (isLive: boolean, isLatest: boolean) => {
  if (isLive && isLatest) {
    return '(Live - Latest release)';
  }
  if (isLive) {
    return '(Live)';
  }
  return '(not Live)';
};

export const getReleaseSummaryLabel = (release: Release) =>
  `${release.title} ${getLiveLatestLabel(release.live, release.latestRelease)}`;
