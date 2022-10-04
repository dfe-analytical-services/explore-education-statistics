import client from '@admin/services/utils/service';
import { IdTitlePair } from '@admin/services/types/common';

export type MethodologyStatus = 'Draft' | 'Approved';
export type MethodologyPublishingStrategy = 'WithRelease' | 'Immediately';

export type UpdateMethodology = {
  latestInternalReleaseNote?: string;
  title: string;
  status: MethodologyStatus;
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
  status: MethodologyStatus;
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
    canMarkMethodologyAsDraft: boolean;
    canMakeAmendmentOfMethodology: boolean;
    canRemoveMethodologyLink: boolean;
  };
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

  listMethodologyVersions(
    publicationId: string,
  ): Promise<MethodologyVersionSummary[]> {
    return client.get(`/publication/${publicationId}/methodologies`);
  },

  createMethodologyAmendment(
    methodologyId: string,
  ): Promise<MethodologyVersion> {
    return client.post(`/methodology/${methodologyId}/amendment`);
  },

  deleteMethodology(methodologyId: string): Promise<void> {
    return client.delete(`/methodology/${methodologyId}`);
  },
};

export default methodologyService;
