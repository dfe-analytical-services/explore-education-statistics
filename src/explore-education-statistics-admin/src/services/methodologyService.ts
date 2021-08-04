import client from '@admin/services/utils/service';

export type UpdateMethodology = {
  title: string;
};

export type MethodologyStatus = 'Draft' | 'Approved';
export type MethodologyPublishingStrategy = 'WithRelease' | 'Immediately';

interface MethodologyPublication {
  id: string;
  title: string;
}

export interface BasicMethodology {
  amendment: boolean;
  id: string;
  internalReleaseNote?: string;
  latestInternalReleaseNote?: string;
  previousVersionId?: string;
  publishingStrategy?: MethodologyPublishingStrategy;
  title: string;
  slug: string;
  status: MethodologyStatus;
  published?: string;
  owningPublication: MethodologyPublication;
  otherPublications?: MethodologyPublication[];
  withReleaseId?: string;
}

export interface MyMethodology extends BasicMethodology {
  permissions: {
    canUpdateMethodology: boolean;
    canDeleteMethodology: boolean;
    canMakeAmendmentOfMethodology: boolean;
  };
}

const methodologyService = {
  createMethodology(publicationId: string): Promise<BasicMethodology> {
    return client.post(`/publication/${publicationId}/methodology`);
  },

  updateMethodology(
    methodologyId: string,
    data: UpdateMethodology,
  ): Promise<BasicMethodology> {
    return client.put(`/methodology/${methodologyId}`, data);
  },

  getMethodology(methodologyId: string): Promise<BasicMethodology> {
    return client.get<BasicMethodology>(
      `/methodology/${methodologyId}/summary`,
    );
  },

  createMethodologyAmendment(methodologyId: string): Promise<BasicMethodology> {
    return client.post(`/methodology/${methodologyId}/amendment`);
  },

  deleteMethodology(methodologyId: string): Promise<void> {
    return client.delete(`/methodology/${methodologyId}`);
  },
};

export default methodologyService;
