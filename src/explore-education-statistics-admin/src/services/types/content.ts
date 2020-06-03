import { ContentBlock, DataBlock } from '@common/services/types/blocks';

export type CommentState = 'open' | 'resolved';

export interface ExtendedComment {
  id: string;
  userId: string;
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

export type ContentBlockPutModel = Pick<EditableContentBlock, 'body'>;
export type ContentBlockPostModel = Pick<
  EditableContentBlock,
  'order' | 'type' | 'body'
>;
