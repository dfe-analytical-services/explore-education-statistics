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
    caption: string;
    content: ContentBlock[];
  }[];
  annexes: {
    order: number;
    heading: string;
    caption: string;
    content: ContentBlock[];
  }[];
  notes: {
    id: string;
    content: string;
    displayDate: Date;
  }[];
}

const methodologyService = {
  getMethodology(methodologySlug: string): Promise<Methodology> {
    return contentApi.get(`/methodologies/${methodologySlug}`);
  },
};
export default methodologyService;
