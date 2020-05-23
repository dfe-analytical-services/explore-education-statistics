import { IdTitlePair } from '@admin/services/common/types';
import { ReleaseStatus } from '@common/services/publicationService';
import { DayMonthYear } from '@common/utils/date/dayMonthYear';

export interface ReleaseSummaryDetails {
  id: string;
  timePeriodCoverage: {
    value: string;
    label: string;
  };
  releaseName: string;
  type: IdTitlePair;
  publishScheduled: string;
  nextReleaseDate?: DayMonthYear;
  status: ReleaseStatus;
  yearTitle: string;
}

export interface BaseReleaseSummaryDetailsRequest {
  timePeriodCoverage: {
    value: string;
  };
  releaseName: number;
  typeId: string;
  publishScheduled: Date;
  nextReleaseDate?: DayMonthYear;
}

export interface ReleasePublicationStatus {
  status: ReleaseStatus;
  amendment: boolean;
  live: boolean;
}

export default {};
