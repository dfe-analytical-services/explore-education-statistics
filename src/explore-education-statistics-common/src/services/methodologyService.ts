import { Publication } from '@common/services/publicationService';
import { ContentBlock } from '@common/services/types/blocks';
import { contentApi } from './api';

export interface Methodology {
  id: string;
  title: string;
  published: string;
  lastUpdated: string;
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
  getMethodology(methodologySlug: string): Promise<Methodology> {
    return contentApi.get(`/methodologies/${methodologySlug}`);
  },
};
