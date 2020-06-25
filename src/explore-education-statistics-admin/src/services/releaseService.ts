import { ContactDetails } from '@admin/services/contactService';
import { IdTitlePair, ValueLabelPair } from '@admin/services/types/common';
import client from '@admin/services/utils/service';
import { ReleaseApprovalStatus } from '@common/services/publicationService';
import { PartialDate } from '@common/utils/date/partialDate';

export interface Release {
  id: string;
  status: ReleaseApprovalStatus;
  latestRelease: boolean;
  live: boolean;
  amendment: boolean;
  releaseName: string;
  publicationId: string;
  publicationTitle: string;
  timePeriodCoverage: ValueLabelPair;
  title: string;
  contact: ContactDetails;
  publishScheduled: string;
  published?: string;
  nextReleaseDate: PartialDate;
  internalReleaseNote?: string;
  previousVersionId: string;
  permissions: {
    canAddPrereleaseUsers: boolean;
    canUpdateRelease: boolean;
    canDeleteRelease: boolean;
    canMakeAmendmentOfRelease: boolean;
  };
}

export interface ReleaseSummary {
  id: string;
  timePeriodCoverage: {
    value: string;
    label: string;
  };
  releaseName: string;
  type: IdTitlePair;
  publishScheduled: string;
  nextReleaseDate?: PartialDate;
  internalReleaseNote: string;
  status: ReleaseApprovalStatus;
  yearTitle: string;
}

type PublishingStage =
  | 'Validating'
  | 'Cancelled'
  | 'Complete'
  | 'Failed'
  | 'Scheduled'
  | 'NotStarted'
  | 'Started';

type OverallStage =
  | 'Validating'
  | 'Complete'
  | 'Failed'
  | 'Invalid'
  | 'Scheduled'
  | 'Started'
  | 'Superseded';

type TaskStage =
  | 'Validating'
  | 'Cancelled'
  | 'Complete'
  | 'Failed'
  | 'Queued'
  | 'NotStarted'
  | 'Started';

export interface ReleaseStageStatuses {
  releaseId?: string;
  dataStage?: TaskStage;
  contentStage?: TaskStage;
  filesStage?: TaskStage;
  publishingStage?: PublishingStage;
  overallStage: OverallStage;
  lastUpdated?: string;
}

export interface ReleasePublicationStatus {
  publishScheduled: string;
  nextReleaseDate?: PartialDate;
  status: ReleaseApprovalStatus;
  amendment: boolean;
  live: boolean;
}

interface BaseReleaseSummaryRequest {
  timePeriodCoverage: {
    value: string;
  };
  releaseName: number;
  typeId: string;
}

export interface CreateReleaseRequest extends BaseReleaseSummaryRequest {
  publicationId: string;
  templateReleaseId?: string;
}

export interface UpdateReleaseSummaryRequest extends BaseReleaseSummaryRequest {
  releaseId: string;
}

export interface UpdateReleaseStatusRequest {
  publishScheduled: string;
  nextReleaseDate?: PartialDate;
  status: ReleaseApprovalStatus;
  internalReleaseNote: string;
}

const releaseService = {
  createRelease(createRequest: CreateReleaseRequest): Promise<ReleaseSummary> {
    return client.post(
      `/publications/${createRequest.publicationId}/releases`,
      createRequest,
    );
  },

  deleteRelease(releaseId: string): Promise<void> {
    return client.delete(`/release/${releaseId}`);
  },

  getDraftReleases(): Promise<Release[]> {
    return client.get<Release[]>('/releases/draft');
  },
  getScheduledReleases(): Promise<Release[]> {
    return client.get<Release[]>('/releases/scheduled');
  },

  async getReleaseSummary(releaseId: string): Promise<ReleaseSummary> {
    return client.get(`/releases/${releaseId}/summary`);
  },

  updateReleaseSummary(
    updateRequest: UpdateReleaseSummaryRequest,
  ): Promise<void> {
    return client.put(
      `/releases/${updateRequest.releaseId}/summary`,
      updateRequest,
    );
  },

  getReleasePublicationStatus(
    releaseId: string,
  ): Promise<ReleasePublicationStatus> {
    return client.get<ReleasePublicationStatus>(
      `/releases/${releaseId}/publication-status`,
    );
  },

  getReleaseStatus(releaseId: string): Promise<ReleaseStageStatuses> {
    return client.get<ReleaseStageStatuses>(`/releases/${releaseId}/status`);
  },

  updateReleaseStatus: (
    releaseId: string,
    updateRequest: UpdateReleaseStatusRequest,
  ): Promise<ReleaseSummary> =>
    client.put(`/releases/${releaseId}/status`, updateRequest),

  createReleaseAmendment(releaseId: string): Promise<ReleaseSummary> {
    return client.post(`/release/${releaseId}/amendment`);
  },
};

export default releaseService;
