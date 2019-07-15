import {DayMonthYearValues, IdLabelPair} from "@admin/services/common/types/types";

export interface ReleaseSetupDetails {
  id: string;
  publicationTitle: string;
  timePeriodCoverageCode: string;
  timePeriodCoverageStartDate: DayMonthYearValues;
  releaseType: IdLabelPair;
  leadStatisticianName: string;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: DayMonthYearValues;
}

export interface ReleaseSetupDetailsUpdateRequest {
  id: string;
  timePeriodCoverageCode: string;
  timePeriodCoverageStartDate: DayMonthYearValues;
  releaseType: IdLabelPair;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: DayMonthYearValues;
}