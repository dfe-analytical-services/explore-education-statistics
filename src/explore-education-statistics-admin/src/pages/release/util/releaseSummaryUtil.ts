import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import { ReleaseStatus } from '@common/services/publicationService';

export const getReleaseStatusLabel = (approvalStatus: ReleaseStatus) => {
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

export const getReleaseSummaryLabel = (release: AdminDashboardRelease) =>
  `${release.title} ${getLiveLatestLabel(release.live, release.latestRelease)}`;
