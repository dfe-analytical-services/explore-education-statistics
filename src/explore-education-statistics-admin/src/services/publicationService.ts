import { User } from '@admin/services/PrototypeLoginService';

export interface IdLabelPair {
  id: string;
  label: string;
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
  scheduledReleaseDate: Date;
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
  scheduledReleaseDate: Date;
  nextExpectedReleaseDate?: Date;
}
