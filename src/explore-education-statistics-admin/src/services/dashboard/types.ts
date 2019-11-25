import {
  ContactDetails,
  IdLabelPair,
  IdTitlePair,
  UserDetails,
} from '@admin/services/common/types';
import {
  DayMonthYearValues,
  ReleaseStatus,
} from '@common/services/publicationService';

export interface ThemeAndTopics {
  title: string;
  id: string;
  topics: {
    title: string;
    id: string;
  }[];
}

export interface Comment {
  message: string;
  authorName: string;
  createdDate: string;
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
  draftComments: Comment[];
  higherReviewComments: Comment[];
}

export interface AdminDashboardPublication {
  id: string;
  title: string;
  methodology?: IdTitlePair;
  releases: AdminDashboardRelease[];
  contact: ContactDetails;
}
