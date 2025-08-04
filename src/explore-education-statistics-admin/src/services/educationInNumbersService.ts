import client from '@admin/services/utils/service';

export interface EducationInNumbersPage {
  description: string;
  id: string;
  published?: string;
  title: string;
  slug: string;
}

export interface CreateEducationInNumbersPageRequest {
  description: string;
  title: string;
}

export interface UpdateEducationInNumbersPageRequest {
  description: string;
  title: string;
  slug?: string;
}

const educationInNumbersService = {
  getEducationInNumbersPages(): Promise<EducationInNumbersPage[]> {
    return client.get('/education-in-numbers');
  },
  getEducationInNumbersPage(
    educationInNumbersPageId: string,
  ): Promise<EducationInNumbersPage> {
    // return client.get(`/education-in-numbers/${educationInNumbersPageId}`);
    return new Promise(resolve => {
      resolve({
        id: educationInNumbersPageId,
        title: 'Key Statistics',
        slug: 'key-statistics',
        description: 'A summary of key statistics in education.',
        published: '2023-10-01T00:00:00Z',
      });
    });
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
};

export default educationInNumbersService;
