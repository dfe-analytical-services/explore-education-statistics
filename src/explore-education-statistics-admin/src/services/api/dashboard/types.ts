import { User } from '@admin/services/PrototypeLoginService';
import {
  IdLabelPair,
  UserContact,
} from '@admin/services/api/common/types/types';

export interface ThemeAndTopics {
  title: string;
  id: string;
  topics: {
    title: string;
    id: string;
  }[];
}

export interface AdminDashboardRelease {
  dateRangeLabel: string;
  timePeriodCoverage: IdLabelPair;
  scheduledPublishDate: Date;
  nextReleaseExpectedDate: Date;
  leadStatistician: UserContact;
  lastEditedUser: User;
  lastEditedDateTime: Date;
}

export interface AdminDashboardPublication {
  id: string;
  title: string;
  methodology: IdLabelPair;
  releases: AdminDashboardRelease[];
}
