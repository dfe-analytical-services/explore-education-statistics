import client from '@admin/services/utils/service';
import { IdTitlePair } from 'src/services/types/common';

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
  title: string;
  slug: string;
  status: MethodologyStatus;
  published?: string;
}

const methodologyService = {
  getMethodologies(): Promise<BasicMethodology[]> {
    // TODO EES-2153 This was returning a list of all methodologies but will need removing
    // when the Manage Publication page no longer offers that list
    return Promise.resolve([]);
  },

  getMyMethodologies(): Promise<MethodologyStatusListItem[]> {
    return client.get<MethodologyStatusListItem[]>('/me/methodologies');
  },

  createMethodology(publicationId: string): Promise<IdTitlePair> {
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
};

export default methodologyService;
