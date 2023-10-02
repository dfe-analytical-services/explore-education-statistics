import client from '@admin/services/utils/service';
import { IdTitlePair } from '@admin/services/types/common';

export type MethodologyApprovalStatus =
  | 'Draft'
  | 'HigherLevelReview'
  | 'Approved';
export type MethodologyPublishingStrategy = 'WithRelease' | 'Immediately';

export type UpdateMethodology = {
  latestInternalReleaseNote?: string;
  title: string;
  status: MethodologyApprovalStatus;
  publishingStrategy?: MethodologyPublishingStrategy;
  withReleaseId?: string;
};

interface MethodologyPublication {
  id: string;
  title: string;
}

export interface BaseMethodologyVersion {
  id: string;
  amendment: boolean;
  previousVersionId?: string;
  title: string;
  status: MethodologyApprovalStatus;
  published?: string;
  methodologyId: string;
}

export interface MethodologyVersion extends BaseMethodologyVersion {
  internalReleaseNote?: string;
  publishingStrategy?: MethodologyPublishingStrategy;
  scheduledWithRelease?: IdTitlePair;
  slug: string;
  owningPublication: MethodologyPublication;
  otherPublications?: MethodologyPublication[];
}

export interface MethodologyVersionSummary extends BaseMethodologyVersion {
  owned: boolean;
  permissions: {
    canDeleteMethodology: boolean;
    canUpdateMethodology: boolean;
    canApproveMethodology: boolean;
    canSubmitMethodologyForHigherReview: boolean;
    canMarkMethodologyAsDraft: boolean;
    canMakeAmendmentOfMethodology: boolean;
    canRemoveMethodologyLink: boolean;
  };
}

export interface MethodologyStatus {
  methodologyStatusId: string;
  internalReleaseNote: string;
  approvalStatus: MethodologyApprovalStatus;
  created: string;
  createdByEmail?: string;
  methodologyVersion: number;
}

const methodologyService = {
  createMethodology(publicationId: string): Promise<MethodologyVersion> {
    return client.post(`/publication/${publicationId}/methodology`);
  },

  updateMethodology(
    methodologyId: string,
    data: UpdateMethodology,
  ): Promise<MethodologyVersion> {
    return client.put(`/methodology/${methodologyId}`, data);
  },

  getMethodology(methodologyId: string): Promise<MethodologyVersion> {
    return client.get<MethodologyVersion>(`/methodology/${methodologyId}`);
  },

  getUnpublishedReleases(methodologyId: string): Promise<IdTitlePair[]> {
    return client.get<IdTitlePair[]>(
      `/methodology/${methodologyId}/unpublished-releases`,
    );
  },

  listLatestMethodologyVersions(
    publicationId: string,
    isPrerelease = false,
  ): Promise<MethodologyVersionSummary[]> {
    return client.get(`/publication/${publicationId}/methodologies`, {
      params: { isPrerelease },
    });
  },

  listMethodologiesForApproval(): Promise<MethodologyVersion[]> {
    return client.get('/methodology/approvals');
  },

  createMethodologyAmendment(
    methodologyId: string,
  ): Promise<MethodologyVersion> {
    return client.post(`/methodology/${methodologyId}/amendment`);
  },

  deleteMethodology(methodologyId: string): Promise<void> {
    return client.delete(`/methodology/${methodologyId}`);
  },
  getMethodologyStatuses(
    methodologyVersionId: string,
  ): Promise<MethodologyStatus[]> {
    return client.get(`/methodology/${methodologyVersionId}/status`);
  },
};

export default methodologyService;
