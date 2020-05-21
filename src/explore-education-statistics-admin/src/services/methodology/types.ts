import { MethodologyStatus } from '@admin/services/common/types';
import { ContentSection } from '@common/services/publicationService';
import { EditableContentBlock } from '../publicationService';

export interface MethodologyStatusListItem {
  id: string;
  title: string;
  status: string;
  publications: MethodologyStatusPublication[];
}

export interface MethodologyStatusPublication {
  id: string;
  title: string;
}

interface SaveMethodologySummary {
  title: string;
  publishScheduled: Date;
  contactId: string;
}

export type CreateMethodology = SaveMethodologySummary;
export type UpdateMethodology = SaveMethodologySummary;

export interface MethodologyContent {
  id: string;
  title: string;
  slug: string;
  status: MethodologyStatus;
  published?: string;
  publishScheduled: string;
  content: ContentSection<EditableContentBlock>[];
  annexes: ContentSection<EditableContentBlock>[];
}

export interface UpdateMethodologyStatusRequest {
  status: MethodologyStatus;
  internalReleaseNote: string;
}
