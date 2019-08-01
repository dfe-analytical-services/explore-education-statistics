import {ReleaseSetupDetails} from "@admin/services/release/types";
import { Dictionary } from '@common/types';

const setupByReleaseId: Dictionary<ReleaseSetupDetails> = {
  'my-publication-1-release-1': {
    id: 'my-publication-1-release-1',
    publicationTitle:
      'Pupil absence statistics and data for schools in England',
    timePeriodCoverageCode: 'AY',
    timePeriodCoverageStartYear: 2017,
    releaseType: {
      id: 'national-stats',
      title: 'National Statistics',
    },
    leadStatisticianName: 'John Smith',
    scheduledPublishDate: {
      day: 20,
      month: 9,
      year: 2018,
    },
    nextReleaseExpectedDate: {
      day: 20,
      month: 9,
      year: 2019,
    },
  },
};

export default {
  setupByReleaseId,
  getReleaseSetupDetailsForRelease: (releaseId: string) =>
    setupByReleaseId[releaseId],
};
