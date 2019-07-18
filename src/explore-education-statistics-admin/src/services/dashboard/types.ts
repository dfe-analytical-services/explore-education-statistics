import {
  DayMonthYearValues,
  IdLabelPair,
  ContactDetails, UserDetails,
} from '@admin/services/common/types';
import { User } from '@admin/services/PrototypeLoginService';

export enum ReleaseApprovalStatus {
  Approved,
  ReadyToReview,
}

export interface ThemeAndTopics {
  title: string;
  id: string;
  topics: {
    title: string;
    id: string;
  }[];
}

export interface AdminDashboardRelease {
  id: string;
  status: ReleaseApprovalStatus;
  latestRelease: boolean;
  live: boolean;
  releaseName: string;
  timePeriodCoverage: IdLabelPair;
  contact: ContactDetails;
  lastEditedUser: UserDetails;
  lastEditedDateTime: string;
  publishScheduled: DayMonthYearValues;
  published?: string;
  nextReleaseExpectedDate: DayMonthYearValues;
}

export interface AdminDashboardPublication {
  id: string;
  title: string;
  methodology: IdLabelPair;
  releases: AdminDashboardRelease[];
  contact: ContactDetails;
}
