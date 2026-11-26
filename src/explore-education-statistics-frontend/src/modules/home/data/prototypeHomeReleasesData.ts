// This file is just for the prototype; providing hardcoded data,
// taken from the live service.
// Further dev should replace this with dynamic data.
import { ReleaseType } from '@common/services/types/releaseType';

export interface HomeRelease {
  publicationSlug: string;
  publicationSummary: string;
  publicationTitle: string;
  publishedDate: string;
  releaseSlug: string;
  releaseTitle: string;
  theme: string;
  type: ReleaseType;
}

export const latestReleases: HomeRelease[] = [
  {
    publicationSlug: 'student-loan-forecasts-earnings',
    publicationSummary:
      'Ad hoc experimental statistics publication presenting analysis of earnings and lifetime repayments forecasts for England',
    publicationTitle: 'Student loan forecasts - earnings',
    publishedDate: '2026-08-27T08:30:03Z',
    releaseSlug: '2025-26',
    releaseTitle: 'Financial year 2025-26',
    theme: 'Finance and funding',
    type: 'AdHocStatistics',
  },
  {
    publicationSlug: 'provisional-t-level-results',
    publicationSummary:
      'A summary of outcomes for students in receipt of T Level results, as reported to the Department for Education through the manage T Level results service.',
    publicationTitle: 'Provisional T Level results',
    publishedDate: '2026-08-13T08:30:01Z',
    releaseSlug: '2025-26-provisional',
    releaseTitle: 'Academic year 2025/26 Provisional',
    theme: 'School and college outcomes and performance',
    type: 'OfficialStatisticsInDevelopment',
  },
  {
    publicationSlug: 'admission-appeals-in-england',
    publicationSummary:
      'Appeals submitted by parents against their child not getting an offer to a preferred primary or secondary school for the start of the academic year.',
    publicationTitle: 'Admission appeals in England',
    publishedDate: '2026-08-06T08:30:03Z',
    releaseSlug: '2026',
    releaseTitle: 'Reporting year 2026',
    theme: 'Pupils and schools',
    type: 'AccreditedOfficialStatistics',
  },
];
