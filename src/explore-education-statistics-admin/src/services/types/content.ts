import { ContentBlock, DataBlock } from '@common/services/types/blocks';

export interface ExtendedComment {
  id: string;
  content: string;
  created: Date;
  createdById: string;
  createdByName: string;
  updated?: Date;
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
