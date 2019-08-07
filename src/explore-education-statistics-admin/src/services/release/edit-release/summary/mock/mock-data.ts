import { ReleaseSummaryDetails } from '@admin/services/release/types';
import { Dictionary } from '@common/types';

const releaseSummaryByReleaseId: Dictionary<ReleaseSummaryDetails> = {
  'my-publication-1-release-1': {
    id: 'my-publication-1-release-1',
    timePeriodCoverageCode: 'AY',
    releaseName: 2017,
    typeId: 'national-statistics',
    publishScheduled: new Date(2018, 9, 20).toISOString(),
    nextReleaseDate: {
      day: 20,
      month: 9,
      year: 2019,
    },
  },
};

export default {
  setupByReleaseId: releaseSummaryByReleaseId,
  getReleaseSummaryDetailsForRelease: (releaseId: string) =>
    releaseSummaryByReleaseId[releaseId],
};
