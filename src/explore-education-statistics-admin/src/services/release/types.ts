import { DayMonthYearValues } from '@admin/services/common/types';

export interface ReleaseSummaryDetails {
  id: string;
  timePeriodCoverageCode: string;
  releaseName: number;
  typeId: string;
  publishScheduled: string;
  nextReleaseDate: DayMonthYearValues;
}

export interface BaseReleaseSummaryDetailsRequest {
  timePeriodCoverage: {
    value: string;
  };
  releaseName: number;
  releaseTypeId: string;
  publishScheduled: Date;
  nextReleaseDate: DayMonthYearValues;
}

export default {};
