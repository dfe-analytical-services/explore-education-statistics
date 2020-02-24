import { ContentBlock } from '@common/services/publicationService';

export interface MethodologyStatus {
  id: string;
  title: string;
  status: string;
  publications: MethodologyStatusPublication[];
}

export interface MethodologyStatusPublication {
  id: string;
  title: string;
}

export interface CreateMethodologyRequest {
  title: string;
  publishScheduled: Date;
  contactId: string;
}

export interface MethodologyContent {
  id: string;
  title: string;
  status: string;
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
