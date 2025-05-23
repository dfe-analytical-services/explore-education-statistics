import { ReleaseVersionSummary } from '@admin/services/releaseVersionService';
import { PaginatedList } from '@common/services/types/pagination';

// eslint-disable-next-line import/prefer-default-export
export const testReleaseSummaries: ReleaseVersionSummary[] = [
  {
    id: 'release-1-version-1',
    releaseId: 'release-1',
    slug: 'release-1-slug',
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic year',
    },
    title: 'Academic year 2023/24',
    type: 'AdHocStatistics',
    approvalStatus: 'Draft',
    year: 2023,
    yearTitle: '2023/24',
    live: false,
    amendment: false,
    latestRelease: false,
  },
  {
    id: 'release-2-version-1',
    releaseId: 'release-2',
    slug: 'release-2-slug',
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic year',
    },
    title: 'Academic year 2022/23',
    type: 'AdHocStatistics',
    approvalStatus: 'Draft',
    year: 2022,
    yearTitle: '2022/23',
    live: false,
    amendment: false,
    latestRelease: false,
  },
  {
    id: 'release-3-version-1',
    releaseId: 'release-3',
    slug: 'release-3-slug',
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic year',
    },
    title: 'Academic year 2021/22',
    type: 'AdHocStatistics',
    publishScheduled: '2021-12-01',
    approvalStatus: 'Approved',
    year: 2021,
    yearTitle: '2021/22',
    live: true,
    amendment: false,
    latestRelease: false,
  },
];

export const testPaginatedReleaseSummaries: PaginatedList<ReleaseVersionSummary> =
  {
    results: testReleaseSummaries,
    paging: {
      page: 1,
      pageSize: 5,
      totalPages: 1,
      totalResults: 3,
    },
  };

export const testPaginatedReleaseSummariesNoResults: PaginatedList<ReleaseVersionSummary> =
  {
    results: [],
    paging: { page: 1, pageSize: 1, totalPages: 1, totalResults: 0 },
  };
