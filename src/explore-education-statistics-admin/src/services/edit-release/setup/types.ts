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

export interface BaseReleaseSetupDetailsRequest {
  timePeriodCoverageCode: string;
  timePeriodCoverageStartDate: DayMonthYearValues;
  releaseType: IdTitlePair;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: DayMonthYearValues;
}

export interface UpdateReleaseSetupDetailsRequest
  extends BaseReleaseSetupDetailsRequest {
  releaseId: string;
}

// TODO - move to different location
export interface CreateReleaseRequest extends BaseReleaseSetupDetailsRequest {
  publicationId: string;
}
