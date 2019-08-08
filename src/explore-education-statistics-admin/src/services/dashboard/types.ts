import {
  ContactDetails,
  DayMonthYearValues,
  IdLabelPair,
  IdTitlePair,
  UserDetails,
} from '@admin/services/common/types';

export enum ReleaseApprovalStatus {
  Approved,
  ReadyToReview,
  None,
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
  publishScheduled: Date;
  published?: string;
  nextReleaseDate: DayMonthYearValues;
}

export interface AdminDashboardPublication {
  id: string;
  title: string;
  methodology?: IdTitlePair;
  releases: AdminDashboardRelease[];
  contact: ContactDetails;
}
