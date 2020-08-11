import { ContactDetails } from '@admin/services/contactService';
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

interface SaveMethodologySummary {
  title: string;
  contactId: string;
}

export type CreateMethodology = SaveMethodologySummary;
export type UpdateMethodology = SaveMethodologySummary;

export type MethodologyStatus = 'Draft' | 'Approved';

export interface BasicMethodology {
  id: string;
  title: string;
  slug: string;
  status: MethodologyStatus;
  // TODO: EES-899 methodology should have a contact attached
  contact?: ContactDetails;
  published?: string;
}

const methodologyService = {
  getMethodologies(): Promise<BasicMethodology[]> {
    return client.get<BasicMethodology[]>('/methodologies');
  },

  getBauMethodologies(): Promise<MethodologyStatusListItem[]> {
    return client.get<MethodologyStatusListItem[]>('/bau/methodology');
  },

  createMethodology(data: CreateMethodology): Promise<IdTitlePair> {
    return client.post(`/methodologies/`, data);
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
