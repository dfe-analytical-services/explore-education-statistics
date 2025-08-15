import { EditableContentBlock } from '@admin/services/types/content';
import { ContentSection } from '@common/services/publicationService';

type BlockMeta = {
  sectionId: string;
  blockId: string;
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
  };
};

export type SetEducationInNumbersPageContent = {
  type: 'SET_CONTENT';
  payload: {
    content: ContentSection<EditableContentBlock>[];
  };
};

export type UpdateContentSection = {
  type: 'UPDATE_CONTENT_SECTION';
  payload: {
    meta: { sectionId: string };
    section: ContentSection<EditableContentBlock>;
  };
};

export type EducationInNumbersPageDispatchAction =
  | RemoveBlockFromSection
  | UpdateBlockFromSection
  | AddBlockToSection
  | UpdateSectionContent
  | AddContentSection
  | SetEducationInNumbersPageContent
  | UpdateContentSection;
