import {
  EditableBlock,
  EditableContentBlock,
} from '@admin/services/types/content';
import { ContentSection, Release } from '@common/services/publicationService';
import { DataBlock } from '@common/services/types/blocks';

export type ContentSectionKeys = keyof Pick<
  Release<EditableContentBlock>,
  | 'summarySection'
  | 'keyStatisticsSection'
  | 'keyStatisticsSecondarySection'
  | 'headlinesSection'
  | 'content'
>;

type BlockMeta = {
  sectionId: string;
  blockId: string;
  sectionKey: ContentSectionKeys;
};

type SectionMeta = Omit<BlockMeta, 'blockId'>;

type SetAvailableDatablocks = {
  type: 'SET_AVAILABLE_DATABLOCKS';
  payload: DataBlock[];
};

export type RemoveBlockFromSection = {
  type: 'REMOVE_BLOCK_FROM_SECTION';
  payload: {
    meta: BlockMeta;
  };
};

export type UpdateBlockFromSection = {
  type: 'UPDATE_BLOCK_FROM_SECTION';
  payload: {
    block: EditableBlock;
    meta: BlockMeta;
  };
};

export type AddBlockToSection = {
  type: 'ADD_BLOCK_TO_SECTION';
  payload: {
    block: EditableBlock;
    meta: SectionMeta;
  };
};

export type UpdateSectionContent = {
  type: 'UPDATE_SECTION_CONTENT';
  payload: {
    sectionContent: EditableBlock[];
    meta: SectionMeta;
  };
};

export type AddContentSection = {
  type: 'ADD_CONTENT_SECTION';
  payload: {
    section: ContentSection<EditableBlock>;
  };
};

export type SetReleaseContent = {
  type: 'SET_CONTENT';
  payload: {
    content: ContentSection<EditableBlock>[];
  };
};

export type UpdateContentSection = {
  type: 'UPDATE_CONTENT_SECTION';
  payload: {
    meta: { sectionId: string };
    section: ContentSection<EditableBlock>;
  };
};

export type ReleaseDispatchAction =
  | SetAvailableDatablocks
  | RemoveBlockFromSection
  | UpdateBlockFromSection
  | AddBlockToSection
  | UpdateSectionContent
  | AddContentSection
  | SetReleaseContent
  | UpdateContentSection;
