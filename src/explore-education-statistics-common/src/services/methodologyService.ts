import { Publication } from '@common/services/publicationService';
import { ContentBlock } from '@common/services/types/blocks';
import { contentApi } from './api';

export interface Topic {
  id: string;
  slug: string;
  title: string;
  summary: string;
  publications: Publication[];
}

export interface Theme {
  id: string;
  slug: string;
  title: string;
  summary: string;
  topics: Topic[];
}

export interface Methodology {
  id: string;
  title: string;
  published: string;
  lastUpdated: string;
  summary: string;
  slug: string;
  publicationId: string;
  publication: Publication;
  content: {
    order: number;
    heading: string;
    caption: string;
    content: ContentBlock[];
  }[];
  annexes: {
    order: number;
    heading: string;
    caption: string;
    content: ContentBlock[];
  }[];
}

export default {
  getMethodologies(): Promise<Theme[]> {
    return contentApi.get(`Methodology/tree`);
  },
  getMethodology(methodologySlug: string): Promise<Methodology> {
    return contentApi.get(`Methodology/${methodologySlug}`);
  },
};
