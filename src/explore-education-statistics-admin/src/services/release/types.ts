import { DayMonthYearValues, IdTitlePair } from '@admin/services/common/types';
import { ReleaseStatus } from '@admin/services/dashboard/types';

export interface ReleaseSummaryDetails {
  id: string;
  timePeriodCoverage: {
    value: string;
    label: string;
  };
  releaseName: string;
  type: IdTitlePair;
  publishScheduled: string;
  nextReleaseDate?: DayMonthYearValues;
  status: ReleaseStatus;
}

export interface BaseReleaseSummaryDetailsRequest {
  timePeriodCoverage: {
    value: string;
  };
  releaseName: number;
  typeId: string;
  publishScheduled: Date;
  nextReleaseDate: DayMonthYearValues;
}

export default {};
