import { DayMonthYearValues, IdTitlePair } from '@admin/services/common/types';

export interface ReleaseSetupDetails {
  id: string;
  publicationTitle: string;
  timePeriodCoverageCode: string;
  timePeriodCoverageStartYear: number;
  releaseType: IdTitlePair;
  leadStatisticianName: string;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: DayMonthYearValues;
}

export interface BaseReleaseSetupDetailsRequest {
  timePeriodCoverage: {
    value: string;
  };
  releaseName: number;
  releaseTypeId: string;
  publishScheduled: DayMonthYearValues;
  nextReleaseExpected: DayMonthYearValues;
}
