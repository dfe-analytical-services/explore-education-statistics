import client from '@admin/services/utils/service';

export interface MethodologyStatusListItem {
  id: string;
  title: string;
  status: string;
  publications: {
    id: string;
    title: string;
  }[];
}

export type UpdateMethodology = {
  title: string;
};

export type MethodologyStatus = 'Draft' | 'Approved';

export interface BasicMethodology {
  amendment: boolean;
  id: string;
  internalReleaseNote?: string;
  live: boolean;
  previousVersionId?: string;
  title: string;
  slug: string;
  status: MethodologyStatus;
  published?: string;
}

export interface MyMethodology extends BasicMethodology {
  permissions: {
    canUpdateMethodology: boolean;
    canCancelMethodologyAmendment: boolean;
    canMakeAmendmentOfMethodology: boolean;
  };
}

const methodologyService = {
  createMethodology(publicationId: string): Promise<BasicMethodology> {
    return client.post(`/publication/${publicationId}/methodology`);
  },

  getMethodologies(): Promise<BasicMethodology[]> {
    // TODO EES-2153 This was returning a list of all methodologies but will need removing
    // when the Manage Publication page no longer offers that list
    return Promise.resolve([]);
  },

  getMyMethodologies(): Promise<MethodologyStatusListItem[]> {
    return client.get<MethodologyStatusListItem[]>('/me/methodologies');
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
