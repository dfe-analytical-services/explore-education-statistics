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
  publish?: boolean;
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
  createEducationInNumbersPageAmendment(
    id: string,
  ): Promise<EducationInNumbersPage> {
    return client.post(`/education-in-numbers/${id}/amendment`);
  },
};

export default educationInNumbersService;
