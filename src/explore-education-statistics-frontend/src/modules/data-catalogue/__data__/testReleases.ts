import { ReleaseSummary } from '@common/services/publicationService';

// eslint-disable-next-line import/prefer-default-export
export const testReleases: ReleaseSummary[] = [
  {
    id: 'release-3',
    latestRelease: true,
    slug: 'release-slug-3',
    title: 'Release title 3',
    yearTitle: 'Release year title 3',
    coverageTitle: 'Release coverage title 3',
    nextReleaseDate: { year: '' },
    type: 'AccreditedOfficialStatistics',
  },
  {
    id: 'release-2',
    latestRelease: false,
    slug: 'release-slug-2',
    title: 'Release title 2',
    yearTitle: 'Release year title 2',
    coverageTitle: 'Release coverage title 2',
    nextReleaseDate: { year: '' },
    type: 'AccreditedOfficialStatistics',
  },
  {
    id: 'release-1',
    latestRelease: false,
    slug: 'release-slug-1',
    title: 'Release title 1',
    yearTitle: 'Release year title 1',
    coverageTitle: 'Release coverage title 1',
    nextReleaseDate: { year: '' },
    type: 'AccreditedOfficialStatistics',
  },
];
