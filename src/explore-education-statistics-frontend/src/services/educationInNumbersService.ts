import { contentApi } from '@common/services/api';
import { EinContentSection } from '@common/services/types/einBlocks';

export interface EinPage {
  id: string;
  title: string;
  slug?: string;
  description: string;
  published: string;
  content: EinContentSection[];
}

export interface EinNavItem {
  id: string;
  title: string;
  order: number;
  slug?: string;
  published: string;
}

const educationInNumbersService = {
  getEducationInNumbersPage(slug: string): Promise<EinPage> {
    return contentApi.get(`/education-in-numbers/${slug}`);
  },
  listEducationInNumbersPages(): Promise<EinNavItem[]> {
    return contentApi.get('/education-in-numbers-nav');
  },
};

export default educationInNumbersService;
