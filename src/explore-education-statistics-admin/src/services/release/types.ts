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
  timePeriodCoverageCode: string;
  timePeriodCoverageStartYear: number;
  releaseType: IdTitlePair;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: DayMonthYearValues;
}