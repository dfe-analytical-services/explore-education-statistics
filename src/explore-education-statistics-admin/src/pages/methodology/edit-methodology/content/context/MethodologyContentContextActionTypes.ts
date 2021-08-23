import { MethodologyContent } from '@admin/services/methodologyContentService';
import { EditableContentBlock } from '@admin/services/types/content';
import { ContentSection } from '@common/services/publicationService';

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
  | RemoveBlockFromSection
  | UpdateBlockFromSection
  | AddBlockToSection
  | UpdateSectionContent
  | AddContentSection
  | SetMethodologyContent
  | UpdateContentSection;
