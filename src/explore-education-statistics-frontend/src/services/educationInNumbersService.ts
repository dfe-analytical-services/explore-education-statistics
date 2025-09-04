import { ContentBlock } from '@common/services/types/blocks';
import { contentApi } from '@common/services/api';

export interface EducationInNumbersPage {
  id: string;
  title: string;
  slug: string;
  description: string;
  published: string;
  content: {
    order: number;
    heading: string;
    caption: string;
    content: ContentBlock[];
  }[];
}

export interface EducationInNumbersNavItem {
  id: string;
  title: string;
  order: number;
  slug: string;
  published: string;
}

const educationInNumbersService = {
  getEducationInNumbersPage(slug: string): Promise<EducationInNumbersPage> {
    return contentApi.get(`/education-in-numbers/${slug}`);
  },
  listEducationInNumbersPages(): Promise<EducationInNumbersNavItem[]> {
    return contentApi.get('/education-in-numbers-nav');
  },
};

export default educationInNumbersService;
