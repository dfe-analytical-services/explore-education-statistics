import {
  EditableContentBlock,
  EditableRelease,
} from '@admin/services/publicationService';
import { DataBlock } from '@common/services/types/blocks';

export interface ManageContentPageViewModel {
  release: EditableRelease;
  availableDataBlocks: DataBlock[];
}

export type ContentBlockPutModel = Pick<EditableContentBlock, 'body'>;
export type ContentBlockPostModel = Pick<
  EditableContentBlock,
  'order' | 'type' | 'body'
>;

export interface ContentBlockAttachRequest {
  contentBlockId: string;
  order: number;
}
