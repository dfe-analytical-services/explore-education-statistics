import { EditableContentBlock } from '@admin/services/publicationService';
import { AbstractRelease } from '@common/services/publicationService';
import { DataBlock } from '@common/services/dataBlockService';

export interface ContentSectionViewModel {
  id: string;
  order: number;
  heading: string;
  caption: string;
  content: EditableContentBlock[];
}

export interface ManageContentPageViewModel {
  release: AbstractRelease<EditableContentBlock>;
  availableDataBlocks: DataBlock[];
}

export interface ContentBlockViewModel {
  id: string;
  order?: number;
  type: string;
  body: string;
}

export type ContentBlockPutModel = Pick<ContentBlockViewModel, 'body'>;
export type ContentBlockPostModel = Pick<
  ContentBlockViewModel,
  'order' | 'type' | 'body'
>;

export interface ContentBlockAttachRequest {
  contentBlockId: string;
  order: number;
}

export type ContentBlockAttachResponse =Pick<ContentBlockViewModel, 'id' | 'type' | 'order'>;

export default {};
