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

export interface ExternalMethodology {
  title: string;
  url: string;
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
  amendment: boolean;
  originalId: string;
  permissions: {
    canUpdateRelease: boolean;
    canMakeAmendmentOfRelease: boolean;
  };
}

export interface AdminDashboardPublication {
  id: string;
  title: string;
  methodology?: IdTitlePair;
  externalMethodology: ExternalMethodology;
  releases: AdminDashboardRelease[];
  contact: ContactDetails;
  permissions: {
    canCreateReleases: boolean;
  };
}
