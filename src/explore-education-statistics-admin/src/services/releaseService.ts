import {
  IdResponse,
  IdTitlePair,
  ValueLabelPair,
} from '@admin/services/types/common';
import client from '@admin/services/utils/service';
import {
  PublicationSummary,
  ReleaseApprovalStatus,
} from '@common/services/publicationService';
import { ReleaseType } from '@common/services/types/releaseType';
import { PartialDate } from '@common/utils/date/partialDate';

export interface ReleasePermissions {
  canAddPrereleaseUsers: boolean;
  canViewRelease: boolean;
  canUpdateRelease: boolean;
  canDeleteRelease: boolean;
  canMakeAmendmentOfRelease: boolean;
}

export interface Release {
  id: string;
  releaseId: string;
  slug: string;
  label?: string;
  version: number;
  approvalStatus: ReleaseApprovalStatus;
  notifySubscribers?: boolean;
  updatePublishedDate: boolean;
  latestRelease: boolean;
  live: boolean;
  amendment: boolean;
  publicationId: string;
  publicationTitle: string;
  publicationSlug: string;
  timePeriodCoverage: ValueLabelPair;
  title: string;
  type: ReleaseType;
  publishScheduled?: string;
  published?: string;
  nextReleaseDate?: PartialDate;
  latestInternalReleaseNote?: string;
  previousVersionId?: string;
  preReleaseAccessList: string;
  preReleaseUsersOrInvitesAdded?: boolean;
  year: number;
  yearTitle: string;
  permissions?: ReleasePermissions;
}

export interface ReleaseSummary {
  id: string;
  title: string;
  slug: string;
  year: number;
  yearTitle: string;
  timePeriodCoverage: ValueLabelPair;
  approvalStatus: ReleaseApprovalStatus;
  publishScheduled?: string;
  published?: string;
  live: boolean;
  nextReleaseDate?: PartialDate;
  type: ReleaseType;
  amendment: boolean;
  latestRelease: boolean;
  previousVersionId?: string;
  permissions?: ReleasePermissions;
  publication?: PublicationSummary;
}

export interface DashboardReleaseSummary extends ReleaseSummaryWithPermissions {
  publication: PublicationSummary;
}

export interface ReleaseSummaryWithPermissions extends ReleaseSummary {
  permissions: ReleasePermissions;
}

interface BaseReleaseRequest {
  year: number;
  timePeriodCoverage: {
    value: string;
  };
  type: ReleaseType;
  label?: string;
}

export interface CreateReleaseRequest extends BaseReleaseRequest {
  publicationId: string;
  templateReleaseId?: string;
}

export interface UpdateReleaseRequest extends BaseReleaseRequest {
  preReleaseAccessList?: string;
}

export interface CreateReleaseStatusRequest {
  approvalStatus: ReleaseApprovalStatus;
  internalReleaseNote?: string;
  publishMethod?: 'Scheduled' | 'Immediate';
  publishScheduled?: string;
  nextReleaseDate?: PartialDate;
  notifySubscribers?: boolean;
  updatePublishedDate?: boolean;
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
  | 'Started'
  | 'Scheduled';

export interface ReleaseStageStatus {
  releaseId?: string;
  contentStage?: TaskStage;
  filesStage?: TaskStage;
  publishingStage?: PublishingStage;
  overallStage: OverallStage;
  lastUpdated?: string;
}

export const ReleaseChecklistErrorCode = [
  'DataFileImportsMustBeCompleted',
  'DataFileReplacementsMustBeCompleted',
  'PublicDataGuidanceRequired',
  'ReleaseNoteRequired',
  'EmptyContentSectionExists',
  'GenericSectionsContainEmptyHtmlBlock',
  'RelatedDashboardsSectionContainsEmptyHtmlBlock',
  'ReleaseMustContainKeyStatOrNonEmptyHeadlineBlock',
  'SummarySectionContainsEmptyHtmlBlock',
  'PublicApiDataSetImportsMustBeCompleted',
  'PublicApiDataSetCancellationsMustBeResolved',
  'PublicApiDataSetFailuresMustBeResolved',
  'PublicApiDataSetMappingsMustBeCompleted',
] as const;

export type ReleaseChecklistError = {
  code: (typeof ReleaseChecklistErrorCode)[number];
};

export type ReleaseChecklistWarning =
  | {
      code:
        | 'NoMethodology'
        | 'NoNextReleaseDate'
        | 'NoDataFiles'
        | 'NoFeaturedTables'
        | 'NoPublicPreReleaseAccessList';
    }
  | {
      code: 'NoFootnotesOnSubjects';
      totalSubjects: number;
    }
  | {
      code: 'MethodologyNotApproved';
      methodologyId: string;
    };

export interface ReleaseChecklist {
  valid: boolean;
  errors: ReleaseChecklistError[];
  warnings: ReleaseChecklistWarning[];
}

export interface ReleasePublicationStatus {
  publishScheduled: string;
  nextReleaseDate?: PartialDate;
  approvalStatus: ReleaseApprovalStatus;
  amendment: boolean;
  live: boolean;
}

export interface ReleaseStatus {
  releaseStatusId: string;
  internalReleaseNote: string;
  approvalStatus: ReleaseApprovalStatus;
  created: string;
  createdByEmail?: string;
  releaseVersion: number;
}

export interface DeleteReleasePlan {
  scheduledMethodologies: IdTitlePair[];
}

const releaseService = {
  createRelease(createRequest: CreateReleaseRequest): Promise<Release> {
    return client.post(
      `/publications/${createRequest.publicationId}/releases`,
      createRequest,
    );
  },

  getRelease(releaseId: string): Promise<Release> {
    return client.get(`/releases/${releaseId}`);
  },

  getReleaseStatuses(releaseId: string): Promise<ReleaseStatus[]> {
    return client.get(`/releases/${releaseId}/status`);
  },

  updateRelease(
    releaseId: string,
    updateRequest: UpdateReleaseRequest,
  ): Promise<Release> {
    return client.put(`/releases/${releaseId}`, updateRequest);
  },

  createReleaseStatus(
    releaseId: string,
    createRequest: CreateReleaseStatusRequest,
  ): Promise<Release> {
    return client.post(`/releases/${releaseId}/status`, createRequest);
  },

  getDeleteReleasePlan(releaseId: string): Promise<DeleteReleasePlan> {
    return client.get<DeleteReleasePlan>(`/release/${releaseId}/delete-plan`);
  },

  deleteRelease(releaseId: string): Promise<void> {
    return client.delete(`/release/${releaseId}`);
  },

  listDraftReleases(): Promise<DashboardReleaseSummary[]> {
    return client.get('/releases/draft');
  },

  listScheduledReleases(): Promise<DashboardReleaseSummary[]> {
    return client.get('/releases/scheduled');
  },

  listReleasesForApproval(): Promise<DashboardReleaseSummary[]> {
    return client.get('/releases/approvals');
  },

  getReleasePublicationStatus(
    releaseId: string,
  ): Promise<ReleasePublicationStatus> {
    return client.get<ReleasePublicationStatus>(
      `/releases/${releaseId}/publication-status`,
    );
  },

  getReleaseStatus(releaseId: string): Promise<ReleaseStageStatus> {
    return client.get<ReleaseStageStatus>(
      `/releases/${releaseId}/stage-status`,
    );
  },

  getReleaseChecklist(releaseId: string): Promise<ReleaseChecklist> {
    return client.get(`/releases/${releaseId}/checklist`);
  },

  createReleaseAmendment(releaseId: string): Promise<IdResponse> {
    return client.post(`/release/${releaseId}/amendment`);
  },
};

export default releaseService;
