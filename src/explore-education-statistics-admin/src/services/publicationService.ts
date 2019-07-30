import {
  ContentBlockType,
  Chart,
  Table,
  Summary,
  AbstractRelease,
} from '@common/services/publicationService';

import { DataBlockRequest } from '@common/services/dataBlockService';

export interface ExtendedComment {
  name: string;
  time: Date;
  comment: string;
  state?: 'open' | 'resolved';
  resolvedBy?: string;
  resolvedOn?: Date;
}

export interface EditableContentBlock {
  type: ContentBlockType;
  body: string;
  heading?: string;
  dataBlockRequest?: DataBlockRequest;
  charts?: Chart[];
  tables?: Table[];
  summary?: Summary;
  comments: ExtendedComment[];
}

export type EditableRelease = AbstractRelease<EditableContentBlock>;
