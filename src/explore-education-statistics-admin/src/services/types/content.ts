import { ContentBlock, DataBlock } from '@common/services/types/blocks';

export interface Comment {
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
  resolved?: string;
  resolvedBy?: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
  };
}

export type EditableContentBlock = ContentBlock & {
  comments: Comment[];
};

export type EditableDataBlock = DataBlock & {
  comments: Comment[];
};

export type EditableBlock = EditableContentBlock | EditableDataBlock;

export type ContentBlockPutModel = Pick<EditableContentBlock, 'body'>;
export type ContentBlockPostModel = Pick<
  EditableContentBlock,
  'order' | 'type' | 'body'
>;
