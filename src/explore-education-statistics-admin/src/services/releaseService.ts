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

export interface ReleaseVersionPermissions {
  canAddPrereleaseUsers: boolean;
  canViewRelease: boolean;
  canUpdateRelease: boolean;
  canDeleteRelease: boolean;
  canMakeAmendmentOfRelease: boolean;
}

export interface ReleaseVersion {
  id: string;
  releaseId: string;
  slug: string;
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
  permissions?: ReleaseVersionPermissions;
}

export interface ReleaseVersionSummary {
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
  permissions?: ReleaseVersionPermissions;
  publication?: PublicationSummary;
}

export interface DashboardReleaseVersionSummary
  extends ReleaseVersionSummaryWithPermissions {
  publication: PublicationSummary;
}

export interface ReleaseVersionSummaryWithPermissions
  extends ReleaseVersionSummary {
  permissions: ReleaseVersionPermissions;
}

interface BaseReleaseVersionRequest {
  year: number;
  timePeriodCoverage: {
    value: string;
  };
  type: ReleaseType;
}

export interface CreateReleaseVersionRequest extends BaseReleaseVersionRequest {
  publicationId: string;
  templateReleaseId?: string;
}

export interface UpdateReleaseVersionRequest extends BaseReleaseVersionRequest {
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
  releaseVersionId?: string;
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
  createRelease(
    createRequest: CreateReleaseVersionRequest,
  ): Promise<ReleaseVersion> {
    return client.post(
      `/publications/${createRequest.publicationId}/releases`,
      createRequest,
    );
  },

  getRelease(releaseVersionId: string): Promise<ReleaseVersion> {
    return client.get(`/releases/${releaseVersionId}`);
  },

  getReleaseStatuses(releaseVersionId: string): Promise<ReleaseStatus[]> {
    return client.get(`/releases/${releaseVersionId}/status`);
  },

  updateRelease(
    releaseVersionId: string,
    updateRequest: UpdateReleaseVersionRequest,
  ): Promise<ReleaseVersion> {
    return client.put(`/releases/${releaseVersionId}`, updateRequest);
  },

  createReleaseStatus(
    releaseVersionId: string,
    createRequest: CreateReleaseStatusRequest,
  ): Promise<ReleaseVersion> {
    return client.post(`/releases/${releaseVersionId}/status`, createRequest);
  },

  getDeleteReleasePlan(releaseVersionId: string): Promise<DeleteReleasePlan> {
    return client.get<DeleteReleasePlan>(
      `/release/${releaseVersionId}/delete-plan`,
    );
  },

  deleteRelease(releaseVersionId: string): Promise<void> {
    return client.delete(`/release/${releaseVersionId}`);
  },

  listDraftReleases(): Promise<DashboardReleaseVersionSummary[]> {
    return client.get('/releases/draft');
  },

  listScheduledReleases(): Promise<DashboardReleaseVersionSummary[]> {
    return client.get('/releases/scheduled');
  },

  listReleasesForApproval(): Promise<DashboardReleaseVersionSummary[]> {
    return client.get('/releases/approvals');
  },

  getReleaseStatus(releaseVersionId: string): Promise<ReleaseStageStatus> {
    return client.get<ReleaseStageStatus>(
      `/releases/${releaseVersionId}/stage-status`,
    );
  },

  getReleaseChecklist(releaseVersionId: string): Promise<ReleaseChecklist> {
    return client.get(`/releases/${releaseVersionId}/checklist`);
  },

  createReleaseAmendment(releaseVersionId: string): Promise<IdResponse> {
    return client.post(`/release/${releaseVersionId}/amendment`);
  },
};

export default releaseService;
