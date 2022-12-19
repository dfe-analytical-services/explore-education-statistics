import { UserDetails } from '@admin/services/types/user';
import {
  ContentBlock,
  DataBlock,
  EmbedBlock,
} from '@common/services/types/blocks';

export interface CommentUser {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
}

export interface Comment {
  id: string;
  content: string;
  createdBy: CommentUser;
  created: string;
  updated?: string;
  resolved?: string;
  resolvedBy?: CommentUser;
  setResolved?: boolean;
}

export type EditableContentBlock = ContentBlock & {
  comments: Comment[];
  locked?: string;
  lockedUntil?: string;
  lockedBy?: UserDetails;
};

export type EditableDataBlock = DataBlock & {
  comments: Comment[];
  locked?: string;
  lockedUntil?: string;
  lockedBy?: UserDetails;
};

export type EditableEmbedBlock = EmbedBlock & {
  comments: Comment[];
  locked?: string;
  lockedUntil?: string;
  lockedBy?: UserDetails;
};

export type EditableBlock =
  | EditableContentBlock
  | EditableDataBlock
  | EditableEmbedBlock;

export type ContentBlockPutModel = Pick<EditableContentBlock, 'body'>;
export type ContentBlockPostModel = Pick<
  EditableContentBlock,
  'order' | 'type' | 'body'
>;

export type EmbedBlockUpdateRequest = {
  title: string;
  url: string;
};

export type EmbedBlockCreateRequest = {
  title: string;
  url: string;
  contentSectionId: string;
};
