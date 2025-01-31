import {
  Comment,
  EditableBlock,
  EditableContentBlock,
} from '@admin/services/types/content';
import {
  ContentSection,
  KeyStatistic,
  Release,
} from '@common/services/publicationService';
import { DataBlock } from '@common/services/types/blocks';

export type ContentSectionKeys = keyof Pick<
  Release<EditableContentBlock>,
  | 'summarySection'
  | 'keyStatisticsSecondarySection'
  | 'headlinesSection'
  | 'relatedDashboardsSection'
  | 'content'
>;

export interface BlockMeta {
  sectionId: string;
  sectionKey: ContentSectionKeys;
  blockId: string;
}

type SectionMeta = Omit<BlockMeta, 'blockId'>;

export type SetUnattachedDataBlocks = {
  type: 'SET_UNATTACHED_DATABLOCKS';
  payload: DataBlock[];
};

export type RemoveSectionBlock = {
  type: 'REMOVE_SECTION_BLOCK';
  payload: {
    meta: BlockMeta;
  };
};

export type UpdateSectionBlock = {
  type: 'UPDATE_SECTION_BLOCK';
  payload: {
    block: EditableBlock;
    meta: BlockMeta;
  };
};

export type AddSectionBlock = {
  type: 'ADD_SECTION_BLOCK';
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

export type AddBlockComment = {
  type: 'ADD_BLOCK_COMMENT';
  payload: {
    comment: Comment;
    meta: BlockMeta;
  };
};

export type UpdateBlockComment = {
  type: 'UPDATE_BLOCK_COMMENT';
  payload: {
    comment: Comment;
    meta: BlockMeta;
  };
};

export type RemoveBlockComment = {
  type: 'REMOVE_BLOCK_COMMENT';
  payload: {
    commentId: string;
    meta: BlockMeta;
  };
};

export type AddKeyStatistic = {
  type: 'ADD_KEY_STATISTIC';
  payload: { keyStatistic: KeyStatistic };
};

export type UpdateKeyStatistic = {
  type: 'UPDATE_KEY_STATISTIC';
  payload: { keyStatistic: KeyStatistic };
};

export type RemoveKeyStatistic = {
  type: 'REMOVE_KEY_STATISTIC';
  payload: { keyStatisticId: string };
};

export type SetKeyStatistics = {
  type: 'SET_KEY_STATISTICS';
  payload: { keyStatistics: KeyStatistic[] };
};

export type ReleaseDispatchAction =
  | AddBlockComment
  | AddSectionBlock
  | AddContentSection
  | AddKeyStatistic
  | SetUnattachedDataBlocks
  | SetReleaseContent
  | SetKeyStatistics
  | RemoveBlockComment
  | RemoveSectionBlock
  | RemoveKeyStatistic
  | UpdateBlockComment
  | UpdateSectionBlock
  | UpdateContentSection
  | UpdateSectionContent
  | UpdateKeyStatistic;
