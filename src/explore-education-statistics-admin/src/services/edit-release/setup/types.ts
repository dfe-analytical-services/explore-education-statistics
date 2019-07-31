import { DayMonthYearValues, IdTitlePair } from '@admin/services/common/types';

export interface ReleaseSetupDetails {
  id: string;
  publicationTitle: string;
  timePeriodCoverageCode: string;
  timePeriodCoverageStartDate: DayMonthYearValues;
  releaseType: IdTitlePair;
  leadStatisticianName: string;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: DayMonthYearValues;
}

export interface ReleaseSetupDetailsUpdateRequest {
  id: string;
  timePeriodCoverageCode: string;
  timePeriodCoverageStartDate: DayMonthYearValues;
  releaseType: IdTitlePair;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: DayMonthYearValues;
}
