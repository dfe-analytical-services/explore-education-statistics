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
  canUpdateRelease: boolean;
  canViewReleaseVersion: boolean;
  canUpdateReleaseVersion: boolean;
  canDeleteReleaseVersion: boolean;
  canMakeAmendmentOfReleaseVersion: boolean;
}

export interface ReleaseVersion {
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
  permissions?: ReleaseVersionPermissions;
}

export interface ReleaseVersionSummary {
  id: string;
  releaseId: string;
  title: string;
  slug: string;
  label?: string;
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

export interface UpdateReleaseVersionRequest {
  preReleaseAccessList?: string;
  year: number;
  timePeriodCoverage: {
    value: string;
  };
  type: ReleaseType;
  label?: string;
}

export interface CreateReleaseVersionStatusRequest {
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

export interface ReleaseVersionStageStatus {
  id?: string;
  contentStage?: TaskStage;
  filesStage?: TaskStage;
  publishingStage?: PublishingStage;
  overallStage: OverallStage;
  lastUpdated?: string;
}

export const ReleaseVersionChecklistErrorCode = [
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

export type ReleaseVersionChecklistError = {
  code: (typeof ReleaseVersionChecklistErrorCode)[number];
};

export type ReleaseVersionChecklistWarning =
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

export interface ReleaseVersionChecklist {
  valid: boolean;
  errors: ReleaseVersionChecklistError[];
  warnings: ReleaseVersionChecklistWarning[];
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

const releaseVersionService = {
  getReleaseVersion(id: string): Promise<ReleaseVersion> {
    return client.get(`/releases/${id}`);
  },

  getReleaseVersionStatuses(id: string): Promise<ReleaseStatus[]> {
    return client.get(`/releases/${id}/status`);
  },

  updateReleaseVersion(
    id: string,
    updateRequest: UpdateReleaseVersionRequest,
  ): Promise<ReleaseVersion> {
    return client.patch(`/releaseVersions/${id}`, updateRequest);
  },

  createReleaseVersionStatus(
    id: string,
    createRequest: CreateReleaseVersionStatusRequest,
  ): Promise<ReleaseVersion> {
    return client.post(`/releases/${id}/status`, createRequest);
  },

  getDeleteReleaseVersionPlan(id: string): Promise<DeleteReleasePlan> {
    return client.get<DeleteReleasePlan>(`/release/${id}/delete-plan`);
  },

  deleteReleaseVersion(id: string): Promise<void> {
    return client.delete(`/release/${id}`);
  },

  listDraftReleaseVersions(): Promise<DashboardReleaseVersionSummary[]> {
    return client.get('/releases/draft');
  },

  listScheduledReleaseVersions(): Promise<DashboardReleaseVersionSummary[]> {
    return client.get('/releases/scheduled');
  },

  listReleaseVersionsForApproval(): Promise<DashboardReleaseVersionSummary[]> {
    return client.get('/releases/approvals');
  },

  getReleaseVersionPublicationStatus(
    id: string,
  ): Promise<ReleasePublicationStatus> {
    return client.get<ReleasePublicationStatus>(
      `/releases/${id}/publication-status`,
    );
  },

  getReleaseVersionStatus(id: string): Promise<ReleaseVersionStageStatus> {
    return client.get<ReleaseVersionStageStatus>(
      `/releases/${id}/stage-status`,
    );
  },

  getReleaseVersionChecklist(id: string): Promise<ReleaseVersionChecklist> {
    return client.get(`/releases/${id}/checklist`);
  },

  createReleaseVersionAmendment(id: string): Promise<IdResponse> {
    return client.post(`/release/${id}/amendment`);
  },
};

export default releaseVersionService;
