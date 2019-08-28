import {
  ContactDetails,
  DayMonthYearValues,
  IdLabelPair,
  IdTitlePair,
  UserDetails,
} from '@admin/services/common/types';

export type ReleaseStatus = 'Draft' | 'HigherLevelReview' | 'Approved';

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
  status: ReleaseStatus;
  latestRelease: boolean;
  live: boolean;
  releaseName: string;
  publicationId: string;
  publicationTitle: string;
  timePeriodCoverage: IdLabelPair;
  contact: ContactDetails;
  lastEditedUser: UserDetails;
  lastEditedDateTime: string;
  publishScheduled: Date;
  published?: string;
  nextReleaseDate: DayMonthYearValues;
  internalReleaseNote?: string;
}

export interface AdminDashboardPublication {
  id: string;
  title: string;
  methodology?: IdTitlePair;
  releases: AdminDashboardRelease[];
  contact: ContactDetails;
}
