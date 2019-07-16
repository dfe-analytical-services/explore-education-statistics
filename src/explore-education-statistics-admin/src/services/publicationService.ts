import { User } from '@admin/services/PrototypeLoginService';
import {
  ContentBlockType,
  Chart,
  Table,
  Summary,
  AbstractRelease,
} from '@common/services/publicationService';
import { DataBlockRequest } from '@common/services/dataBlockService';

export interface Methodology {
  id: string;
  title: string;
}

export interface Theme {
  id: string;
  title: string;
}

export interface Topic {
  id: string;
  title: string;
  theme: Theme;
}

export interface TimePeriod {
  id: string;
  title: string;
}

export interface TimePeriodCoverage {
  label: string;
  academicYear?: {
    yearStarting: number;
    timePeriod: TimePeriod;
    termsPerYear: number;
  };
  calendarYear?: {
    year: number;
  };
  financialYear?: {
    startDate: Date;
    timePeriod: TimePeriod;
  };
  month?: {
    monthlyReleaseDate: Date;
  };
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
  scheduledReleaseDate: Date;
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

export interface UserContact {
  name: string;
  email: string;
  telNo: string;
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
  methodology: Methodology;
  owner: User;
}

export interface ReleaseSetupDetails {
  publicationTitle: string;
  releaseType: string;
  releaseName: string;
  leadStatisticianName: string;
  scheduledReleaseDate: Date;
}

export interface ExtendedComment {
  name: string;
  time: Date;
  comment: string;
  state?: 'open' | 'resolved';
  resolvedBy?: string;
  resolvedOn?: Date;
}

export interface EditableContentBlock {
  type: ContentBlockType;
  body: string;
  heading?: string;
  dataBlockRequest?: DataBlockRequest;
  charts?: Chart[];
  tables?: Table[];
  summary?: Summary;
  comments: ExtendedComment[];
}

export type EditableRelease = AbstractRelease<EditableContentBlock>;
