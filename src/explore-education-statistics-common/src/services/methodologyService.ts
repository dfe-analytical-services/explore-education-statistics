import { PublicationSummary } from '@common/services/publicationService';
import { ContentBlock } from '@common/services/types/blocks';
import { contentApi } from './api';

export interface Methodology {
  id: string;
  title: string;
  published: string;
  slug: string;
  publications: PublicationSummary[];
  content: {
    order: number;
    heading: string;
    content: ContentBlock[];
  }[];
  annexes: {
    order: number;
    heading: string;
    content: ContentBlock[];
  }[];
  notes: {
    id: string;
    content: string;
    displayDate: Date;
  }[];
}

export interface MethodologySitemapItem {
  slug: string;
  lastModified?: string;
}

const methodologyService = {
  getMethodology(methodologySlug: string): Promise<Methodology> {
    return contentApi.get(`/methodologies/${methodologySlug}`);
  },
  listSitemapItems(): Promise<MethodologySitemapItem[]> {
    return contentApi.get('/methodologies/sitemap-items');
  },
};

export default methodologyService;
