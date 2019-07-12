import { User } from '../../../PrototypeLoginService';

export interface IdLabelPair {
  id: string;
  label: string;
}

export interface UserContact {
  name: string;
  email: string;
  telNo: string;
}

export interface Topic {
  id: string;
  title: string;
  theme: IdLabelPair;
}

export interface TimePeriodCoverage {
  label: string;
  code: string;
  startDate: Date;
}

export enum ApprovalStatus {
  Approved,
  ReadyToReview,
}

export interface ReleaseStatus {
  approvalStatus: ApprovalStatus;
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
  releaseType: IdLabelPair;
  slug: string;
  status: ReleaseStatus;
  lead: UserContact;
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
  contact: UserContact;
  methodology: IdLabelPair;
  owner: User;
}

export interface ReleaseSetupDetails {
  id: string;
  publicationTitle: string;
  timePeriodCoverageCode: string;
  timePeriodCoverageStartDate: Date;
  releaseType: IdLabelPair;
  leadStatisticianName: string;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate?: Date;
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

export const dayMonthYearToDate = (dmy: DayMonthYearValues) => {
  if (!(dmy.day && dmy.month && dmy.year)) {
    throw Error(
      `Couldn't convert DayMonthYearValues ${JSON.stringify(
        dmy,
      )} to Date - missing required value`,
    );
  }
  return new Date(dmy.year, dmy.month - 1, dmy.day);
};
