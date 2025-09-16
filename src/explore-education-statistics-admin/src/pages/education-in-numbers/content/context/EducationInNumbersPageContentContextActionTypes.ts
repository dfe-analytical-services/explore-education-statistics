import { EinEditableContentSection } from '@admin/services/educationInNumbersContentService';
import {
  EinContentBlock,
  EinFreeTextStatTile,
} from '@common/services/types/einBlocks';

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
    block: EinContentBlock;
    meta: BlockMeta;
  };
};

export type AddBlockToSection = {
  type: 'ADD_BLOCK_TO_SECTION';
  payload: {
    block: EinContentBlock;
    meta: SectionMeta;
  };
};

export type AddFreeTextStatTileToBlock = {
  type: 'ADD_FREE_TEXT_STAT_TILE_TO_BLOCK';
  payload: {
    tile: EinFreeTextStatTile;
    meta: BlockMeta;
  };
};

export type UpdateSectionContent = {
  type: 'UPDATE_SECTION_CONTENT';
  payload: {
    sectionContent: EinContentBlock[];
    meta: SectionMeta;
  };
};

export type AddContentSection = {
  type: 'ADD_CONTENT_SECTION';
  payload: {
    section: EinEditableContentSection;
  };
};

export type SetEducationInNumbersPageContent = {
  type: 'SET_CONTENT';
  payload: {
    content: EinEditableContentSection[];
  };
};

export type UpdateContentSection = {
  type: 'UPDATE_CONTENT_SECTION';
  payload: {
    meta: { sectionId: string };
    section: EinEditableContentSection;
  };
};

export type EducationInNumbersPageDispatchAction =
  | RemoveBlockFromSection
  | UpdateBlockFromSection
  | AddBlockToSection
  | AddFreeTextStatTileToBlock
  | UpdateSectionContent
  | AddContentSection
  | SetEducationInNumbersPageContent
  | UpdateContentSection;
