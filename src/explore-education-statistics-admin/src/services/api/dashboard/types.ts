import { User } from '@admin/services/PrototypeLoginService';
import {
  DayMonthYearValues,
  IdLabelPair,
  UserContact,
} from '@admin/services/api/common/types/types';

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
  dateRangeLabel: string;
  timePeriodCoverage: IdLabelPair;
  leadStatistician: UserContact;
  lastEditedUser: User;
  lastEditedDateTime: string;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: DayMonthYearValues;
}

export interface AdminDashboardPublication {
  id: string;
  title: string;
  methodology: IdLabelPair;
  releases: AdminDashboardRelease[];
}
