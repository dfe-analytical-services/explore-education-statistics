import { DayMonthYearValues, IdTitlePair } from '@admin/services/common/types';

export interface ReleaseSummaryDetails {
  id: string;
  publicationTitle: string;
  timePeriodCoverageCode: string;
  timePeriodCoverageStartYear: number;
  releaseType: IdTitlePair;
  leadStatisticianName: string;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: DayMonthYearValues;
}

export interface BaseReleaseSummaryDetailsRequest {
  timePeriodCoverage: {
    value: string;
  };
  releaseName: number;
  releaseTypeId: string;
  publishScheduled: Date;
  nextReleaseExpected: DayMonthYearValues;
}

export default {};
