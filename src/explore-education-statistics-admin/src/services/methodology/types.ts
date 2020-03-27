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

export interface CreateMethodologyRequest {
  title: string;
  publishScheduled: Date;
  contactId: string;
}

export interface MethodologyContent {
  id: string;
  title: string;
  status: string;
  published?: string;
  lastUpdated?: string;
  content: ContentSection<EditableContentBlock>[];
  annexes: ContentSection<EditableContentBlock>[];
}

export interface UpdateMethodologyStatusRequest {
  status: MethodologyStatus;
  internalReleaseNote: string;
}
