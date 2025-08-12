import client from '@admin/services/utils/service';

export interface EducationInNumbersPage {
  id: string;
  title: string;
  slug: string;
  description: string;
  version: number;
  published?: string;
  previousVersionId: string;
}

export interface CreateEducationInNumbersPageRequest {
  description: string;
  title: string;
}

export interface UpdateEducationInNumbersPageRequest {
  title?: string;
  slug?: string;
  description?: string;
}

const educationInNumbersService = {
  getEducationInNumbersPage(id: string): Promise<EducationInNumbersPage> {
    return client.get(`/education-in-numbers/${id}`);
  },
  listLatestPages(): Promise<EducationInNumbersPage[]> {
    return client.get('/education-in-numbers');
  },
  createEducationInNumbersPage(
    page: CreateEducationInNumbersPageRequest,
  ): Promise<EducationInNumbersPage> {
    return client.post('/education-in-numbers', page);
  },
  updateEducationInNumbersPage(
    educationInNumbersPageId: string,
    page: UpdateEducationInNumbersPageRequest,
  ): Promise<EducationInNumbersPage> {
    return client.put(
      `/education-in-numbers/${educationInNumbersPageId}`,
      page,
    );
  },
  publishEducationInNumbersPage(id: string): Promise<EducationInNumbersPage> {
    return client.patch(`education-in-numbers/${id}/publish`);
  },
  createEducationInNumbersPageAmendment(
    id: string,
  ): Promise<EducationInNumbersPage> {
    return client.post(`/education-in-numbers/${id}/amendment`);
  },
  deleteEducationInNumbersPage(id: string): Promise<void> {
    return client.delete(`education-in-numbers/${id}`);
  },
};

export default educationInNumbersService;
