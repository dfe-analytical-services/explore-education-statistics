import { Release } from '@common/services/publicationService';
import { ContentBlock, DataBlock } from '@common/services/types/blocks';

export type CommentState = 'open' | 'resolved';

export interface ExtendedComment {
  id: string;
  name: string;
  time: Date;
  commentText: string;
  state?: CommentState;
  resolvedBy?: string;
  resolvedOn?: Date;
}

export type EditableContentBlock = ContentBlock & {
  comments: ExtendedComment[];
};

export type EditableDataBlock = DataBlock & {
  comments: ExtendedComment[];
};

export type EditableBlock = EditableContentBlock | EditableDataBlock;

export type EditableRelease = Release<EditableContentBlock, EditableDataBlock>;
