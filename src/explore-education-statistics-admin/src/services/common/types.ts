import { ReleaseApprovalStatus } from '@admin/services/dashboard/types';
import { User } from '@admin/services/sign-in/types';

export interface IdTitlePair {
  id: string;
  title: string;
}

export interface ContactDetails {
  id: string;
  contactName: string;
  contactTelNo: string;
  teamEmail: string;
  teamName?: string;
}

export interface UserDetails {
  id: string;
  name: string;
}

export interface Topic {
  id: string;
  title: string;
  theme: IdTitlePair;
}

export interface TimePeriodCoverage {
  label: string;
  code: string;
  startDate: Date;
}

export interface ReleaseStatus {
  approvalStatus: ReleaseApprovalStatus;
  isNew: boolean;
  isLive: boolean;
  isLatest: boolean;
  lastEdited: Date;
  lastEditor: User;
  published: Date;
  nextRelease: Date;
}

export interface ReleaseDataType {
  id: string;
  title: string;
}

export interface Comment {
  id: string;
  author: User;
  datetime: Date;
  content: string;
}

export interface Release {
  id: string;
  releaseName: string;
  timePeriodCoverage: TimePeriodCoverage;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: Date;
  releaseType: IdTitlePair;
  slug: string;
  status: ReleaseStatus;
  lead: ContactDetails;
  dataType: ReleaseDataType;
  comments: Comment[];
}

export interface LegacyRelease {
  id: string;
  description: string;
  url: string;
}

export interface Publication {
  id: string;
  slug: string;
  title: string;
  description: string;
  dataSource: string;
  summary: string;
  nextUpdate: string;
  releases: Release[];
  legacyReleases: LegacyRelease[];
  topic: Topic;
  contact: ContactDetails;
  methodology: IdTitlePair;
  owner: User;
}

export interface DayMonthYearValues {
  day?: number;
  month?: number;
  year?: number;
}

export const dateToDayMonthYear = (date?: Date) => {
  return {
    day: date && date.getDate(),
    month: date && date.getMonth() + 1,
    year: date && date.getFullYear(),
  };
};

export const dayMonthYearIsComplete = (dmy?: DayMonthYearValues) => {
  return dmy && dmy.day && dmy.month && dmy.year;
};

export const dayMonthYearToDate = (dmy: DayMonthYearValues) => {
  if (!dayMonthYearIsComplete(dmy)) {
    throw Error(
      `Couldn't convert DayMonthYearValues ${JSON.stringify(
        dmy,
      )} to Date - missing required value`,
    );
  }
  return new Date(dmy.year || 0, (dmy.month || 0) - 1, dmy.day);
};
