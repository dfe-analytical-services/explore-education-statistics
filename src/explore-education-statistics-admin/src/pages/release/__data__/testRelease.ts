import { ReleaseVersion } from '@admin/services/releaseVersionService';

// eslint-disable-next-line import/prefer-default-export
export const testRelease: ReleaseVersion = {
  id: 'release-1',
  releaseId: 'release-series-1',
  slug: 'release-1-slug',
  approvalStatus: 'Draft',
  latestRelease: false,
  live: false,
  amendment: false,
  year: 2021,
  yearTitle: '2021/22',
  publicationId: 'publication-1',
  publicationTitle: 'Publication 1',
  publicationSlug: 'publication-1-slug',
  timePeriodCoverage: { value: 'W51', label: 'Week 51' },
  title: 'Release Title',
  label: undefined,
  version: 1,
  type: 'OfficialStatistics',
  previousVersionId: '',
  preReleaseAccessList: '',
  updatePublishedDate: false,
};
