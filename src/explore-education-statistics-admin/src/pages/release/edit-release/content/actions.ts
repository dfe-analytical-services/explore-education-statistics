import { EditableContentBlock } from '@admin/services/publicationService';
import {
  AbstractRelease,
  ContentSection,
} from '@common/services/publicationService';
import { State } from './ReleaseContext';

export type ContentSectionKeys = keyof Pick<
  AbstractRelease<EditableContentBlock>,
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

type ClearState = { type: 'CLEAR_STATE' };
type SetState = { type: 'SET_STATE'; payload: State };
type SetAvailableDatablocks = {
  type: 'SET_AVAILABLE_DATABLOCKS';
  payload: Pick<State, 'availableDataBlocks'>;
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

export type SetReleaseContent = {
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

type ReleaseDispatchAction =
  | ClearState
  | SetState
  | SetAvailableDatablocks
  | RemoveBlockFromSection
  | UpdateBlockFromSection
  | AddBlockToSection
  | UpdateSectionContent
  | AddContentSection
  | SetReleaseContent
  | UpdateContentSection;

// eslint-disable-next-line no-undef
export default ReleaseDispatchAction;
