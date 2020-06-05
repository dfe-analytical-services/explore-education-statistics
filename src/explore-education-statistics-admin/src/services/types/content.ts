import { ContentBlock, DataBlock } from '@common/services/types/blocks';

export interface ExtendedComment {
  id: string;
  content: string;
  createdBy: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
  };
  created: string;
  updated?: string;
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
