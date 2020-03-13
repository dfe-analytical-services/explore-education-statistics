import { IdTitlePair } from '@admin/services/common/types';
import {
  DayMonthYearValues,
  ReleaseStatus,
} from '@common/services/publicationService';

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
