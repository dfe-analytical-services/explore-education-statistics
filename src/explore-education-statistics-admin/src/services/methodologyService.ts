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

export interface BasicMethodologyVersion {
  amendment: boolean;
  id: string;
  latestInternalReleaseNote?: string;
  previousVersionId?: string;
  publishingStrategy?: MethodologyPublishingStrategy;
  title: string;
  slug: string;
  status: MethodologyStatus;
  published?: string;
  owningPublication: MethodologyPublication;
  otherPublications?: MethodologyPublication[];
  scheduledWithRelease?: IdTitlePair;
  methodologyId: string;
}

export interface MethodologyVersionListItem {
  id: string;
  amendment: boolean;
  published?: string;
  status: MethodologyStatus;
  owned: boolean;
  permissions: {
    canDeleteMethodologyVersion: boolean;
    canUpdateMethodologyVersion: boolean;
    canApproveMethodologyVersion: boolean;
    canMarkMethodologyVersionAsDraft: boolean;
    canMakeAmendmentOfMethodology: boolean;
    canRemoveMethodologyLink: boolean;
  };
  title: string;
  internalReleaseNote?: string;
  methodologyId: string;
  previousVersionId?: string;
}

const methodologyService = {
  createMethodology(publicationId: string): Promise<BasicMethodologyVersion> {
    return client.post(`/publication/${publicationId}/methodology`);
  },

  updateMethodology(
    methodologyId: string,
    data: UpdateMethodology,
  ): Promise<BasicMethodologyVersion> {
    return client.put(`/methodology/${methodologyId}`, data);
  },

  getMethodology(methodologyId: string): Promise<BasicMethodologyVersion> {
    return client.get<BasicMethodologyVersion>(
      `/methodology/${methodologyId}/summary`,
    );
  },

  getUnpublishedReleases(methodologyId: string): Promise<IdTitlePair[]> {
    return client.get<IdTitlePair[]>(
      `/methodology/${methodologyId}/unpublished-releases`,
    );
  },

  listMethodologyVersions(
    publicationId: string,
  ): Promise<MethodologyVersionListItem[]> {
    return client.get(`/publication/${publicationId}/methodologies`);
  },

  createMethodologyAmendment(
    methodologyId: string,
  ): Promise<BasicMethodologyVersion> {
    return client.post(`/methodology/${methodologyId}/amendment`);
  },

  deleteMethodology(methodologyId: string): Promise<void> {
    return client.delete(`/methodology/${methodologyId}`);
  },
};

export default methodologyService;
