import client from '@admin/services/utils/service';

export interface EinSummary {
  id: string;
  title: string;
  slug?: string;
  description: string;
  version: number;
  published?: string;
}

export interface EinSummaryWithPrevVersion extends EinSummary {
  previousVersionId?: string;
}

export interface CreateEducationInNumbersPageRequest {
  description: string;
  title: string;
}

export interface UpdateEducationInNumbersPageRequest {
  title?: string;
  description?: string;
}

const educationInNumbersService = {
  getEducationInNumbersPage(id: string): Promise<EinSummary> {
    return client.get(`/education-in-numbers/${id}`);
  },
  listLatestPages(): Promise<EinSummaryWithPrevVersion[]> {
    return client.get('/education-in-numbers');
  },
  createEducationInNumbersPage(
    page: CreateEducationInNumbersPageRequest,
  ): Promise<EinSummary> {
    return client.post('/education-in-numbers', page);
  },
  updateEducationInNumbersPage(
    educationInNumbersPageId: string,
    page: UpdateEducationInNumbersPageRequest,
  ): Promise<EinSummary> {
    return client.put(
      `/education-in-numbers/${educationInNumbersPageId}`,
      page,
    );
  },
  publishEducationInNumbersPage(id: string): Promise<EinSummary> {
    return client.patch(`/education-in-numbers/${id}/publish`);
  },
  createEducationInNumbersPageAmendment(id: string): Promise<EinSummary> {
    return client.post(`/education-in-numbers/${id}/amendment`);
  },
  deleteEducationInNumbersPage(id: string): Promise<void> {
    return client.delete(`/education-in-numbers/${id}`);
  },
  reorderEducationInNumbersPages(pageIds: string[]): Promise<void> {
    return client.put(`/education-in-numbers/order`, pageIds);
  },
};

export default educationInNumbersService;
