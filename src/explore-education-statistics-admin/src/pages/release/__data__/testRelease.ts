import { Release } from '@admin/services/releaseService';

// eslint-disable-next-line import/prefer-default-export
export const testRelease: Release = {
  id: 'release-1',
  slug: 'release-1-slug',
  approvalStatus: 'Draft',
  latestRelease: false,
  live: false,
  amendment: false,
  year: 2021,
  yearTitle: '2021/22',
  publicationId: 'publication-1',
  publicationTitle: 'Publication 1',
  publicationSummary: 'Publication Summary',
  publicationSlug: 'publication-1-slug',
  timePeriodCoverage: { value: 'W51', label: 'Week 51' },
  title: 'Release Title',
  type: 'OfficialStatistics',
  contact: {
    teamName: 'Test name',
    teamEmail: 'test@test.com',
    contactName: 'Test contact name',
    contactTelNo: '1111 1111 1111',
  },
  previousVersionId: '',
  preReleaseAccessList: '',
  updatePublishedDate: false,
};
