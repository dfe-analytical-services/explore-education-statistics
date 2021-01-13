import { ContactDetails } from '@admin/services/contactService';
import { IdTitlePair, ValueLabelPair } from '@admin/services/types/common';
import client from '@admin/services/utils/service';
import { ReleaseApprovalStatus } from '@common/services/publicationService';
import { PartialDate } from '@common/utils/date/partialDate';

export interface Release {
  id: string;
  slug: string;
  status: ReleaseApprovalStatus;
  latestRelease: boolean;
  live: boolean;
  amendment: boolean;
  releaseName: string;
  publicationId: string;
  publicationTitle: string;
  publicationSlug: string;
  timePeriodCoverage: ValueLabelPair;
  title: string;
  type: IdTitlePair;
  contact: ContactDetails;
  publishScheduled?: string;
  published?: string;
  nextReleaseDate?: PartialDate;
  internalReleaseNote?: string;
  previousVersionId: string;
  preReleaseAccessList: string;
}

export interface MyRelease extends Release {
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

interface BaseReleaseRequest {
  releaseName: string;
  timePeriodCoverage: {
    value: string;
  };
  typeId: string;
}

export interface CreateReleaseRequest extends BaseReleaseRequest {
  publicationId: string;
  templateReleaseId?: string;
}

export interface UpdateReleaseRequest extends BaseReleaseRequest {
  status: ReleaseApprovalStatus;
  internalReleaseNote?: string;
  publishScheduled?: string;
  publishMethod?: 'Scheduled' | 'Immediate';
  nextReleaseDate?: PartialDate;
  preReleaseAccessList?: string;
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

export type ReleaseChecklistError =
  | {
      code:
        | 'DataFileImportsMustBeCompleted'
        | 'DataFileReplacementsMustBeCompleted'
        | 'PublicMetaGuidanceRequired'
        | 'ReleaseNoteRequired';
    }
  | {
      code: 'MethodologyMustBeApproved';
      methodologyId: string;
    };

export type ReleaseChecklistWarning =
  | {
      code:
        | 'NoMethodology'
        | 'NoNextReleaseDate'
        | 'NoDataFiles'
        | 'NoTableHighlights'
        | 'NoPublicPreReleaseAccessList';
    }
  | {
      code: 'NoFootnotesOnSubjects';
      totalSubjects: number;
    };

export interface ReleaseChecklist {
  valid: boolean;
  errors: ReleaseChecklistError[];
  warnings: ReleaseChecklistWarning[];
}

export interface ReleasePublicationStatus {
  publishScheduled: string;
  nextReleaseDate?: PartialDate;
  status: ReleaseApprovalStatus;
  amendment: boolean;
  live: boolean;
}

const releaseService = {
  createRelease(createRequest: CreateReleaseRequest): Promise<ReleaseSummary> {
    return client.post(
      `/publications/${createRequest.publicationId}/releases`,
      createRequest,
    );
  },

  getRelease(releaseId: string): Promise<Release> {
    return client.get(`/releases/${releaseId}`);
  },

  updateRelease(
    releaseId: string,
    updateRequest: UpdateReleaseRequest,
  ): Promise<Release> {
    return client.put(`/releases/${releaseId}`, updateRequest);
  },

  deleteRelease(releaseId: string): Promise<void> {
    return client.delete(`/release/${releaseId}`);
  },
  getDraftReleases(): Promise<MyRelease[]> {
    return client.get('/releases/draft');
  },

  getScheduledReleases(): Promise<MyRelease[]> {
    return client.get('/releases/scheduled');
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

  getReleaseChecklist(releaseId: string): Promise<ReleaseChecklist> {
    return client.get(`/releases/${releaseId}/checklist`);
  },

  createReleaseAmendment(releaseId: string): Promise<ReleaseSummary> {
    return client.post(`/release/${releaseId}/amendment`);
  },
};

export default releaseService;
