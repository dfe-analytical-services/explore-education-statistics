import { IdTitlePair } from '@admin/services/common/types';
import { ReleaseStatus } from '@common/services/publicationService';
import { DayMonthYearValues } from '@common/utils/date/dayMonthYear';

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

export interface ReleasePublicationStatus {
  status: ReleaseStatus;
  amendment: boolean;
  live: boolean;
}

export default {};
