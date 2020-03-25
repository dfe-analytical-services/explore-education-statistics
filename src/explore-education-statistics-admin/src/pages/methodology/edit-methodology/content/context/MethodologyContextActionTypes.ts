import { MethodologyContent } from '@admin/services/methodology/types';
import { EditableContentBlock } from '@admin/services/publicationService';
import { ContentSection } from '@common/services/publicationService';
import { MethodologyContextState } from './MethodologyContext';

export type ContentSectionKeys = keyof Pick<
  MethodologyContent,
  'annexes' | 'content'
>;
type BlockMeta = {
  sectionId: string;
  blockId: string;
  sectionKey: ContentSectionKeys;
};
type SectionMeta = Omit<BlockMeta, 'blockId'>;

type ClearState = { type: 'CLEAR_STATE' };
type SetState = { type: 'SET_STATE'; payload: MethodologyContextState };

export type RemoveBlockFromSection = {
  type: 'REMOVE_BLOCK_FROM_SECTION';
  payload: {
    meta: BlockMeta;
  };
};
export type UpdateBlockFromSection = {
  type: 'UPDATE_BLOCK_FROM_SECTION';
  payload: {
    block: EditableContentBlock;
    meta: BlockMeta;
  };
};

export type AddBlockToSection = {
  type: 'ADD_BLOCK_TO_SECTION';
  payload: {
    block: EditableContentBlock;
    meta: SectionMeta;
  };
};

export type UpdateSectionContent = {
  type: 'UPDATE_SECTION_CONTENT';
  payload: {
    sectionContent: EditableContentBlock[];
    meta: SectionMeta;
  };
};

export type AddContentSection = {
  type: 'ADD_CONTENT_SECTION';
  payload: {
    section: ContentSection<EditableContentBlock>;
    sectionKey: ContentSectionKeys;
  };
};

export type SetMethodologyContent = {
  type: 'SET_CONTENT';
  payload: {
    content: ContentSection<EditableContentBlock>[];
    sectionKey: ContentSectionKeys;
  };
};

export type UpdateContentSection = {
  type: 'UPDATE_CONTENT_SECTION';
  payload: {
    meta: { sectionId: string; sectionKey: ContentSectionKeys };
    section: ContentSection<EditableContentBlock>;
  };
};

export type MethodologyDispatchAction =
  | ClearState
  | SetState
  | RemoveBlockFromSection
  | UpdateBlockFromSection
  | AddBlockToSection
  | UpdateSectionContent
  | AddContentSection
  | SetMethodologyContent
  | UpdateContentSection;
