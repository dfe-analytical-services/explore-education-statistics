import client from '@admin/services/utils/service';

export interface EducationInNumbersSummary {
  id: string;
  title: string;
  slug?: string;
  description: string;
  version: number;
  published?: string;
}

export interface EducationInNumbersSummaryWithPrevVersion
  extends EducationInNumbersSummary {
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
  getEducationInNumbersPage(id: string): Promise<EducationInNumbersSummary> {
    return client.get(`/education-in-numbers/${id}`);
  },
  listLatestPages(): Promise<EducationInNumbersSummaryWithPrevVersion[]> {
    return client.get('/education-in-numbers');
  },
  createEducationInNumbersPage(
    page: CreateEducationInNumbersPageRequest,
  ): Promise<EducationInNumbersSummary> {
    return client.post('/education-in-numbers', page);
  },
  updateEducationInNumbersPage(
    educationInNumbersPageId: string,
    page: UpdateEducationInNumbersPageRequest,
  ): Promise<EducationInNumbersSummary> {
    return client.put(
      `/education-in-numbers/${educationInNumbersPageId}`,
      page,
    );
  },
  publishEducationInNumbersPage(
    id: string,
  ): Promise<EducationInNumbersSummary> {
    return client.patch(`education-in-numbers/${id}/publish`);
  },
  createEducationInNumbersPageAmendment(
    id: string,
  ): Promise<EducationInNumbersSummary> {
    return client.post(`/education-in-numbers/${id}/amendment`);
  },
  deleteEducationInNumbersPage(id: string): Promise<void> {
    return client.delete(`education-in-numbers/${id}`);
  },
};

export default educationInNumbersService;
