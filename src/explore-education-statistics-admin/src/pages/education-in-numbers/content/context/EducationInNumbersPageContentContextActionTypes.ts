import { EinEditableContentSection } from '@admin/services/educationInNumbersContentService';
import { EinContentBlock, EinTile } from '@common/services/types/einBlocks';

type BlockMeta = {
  sectionId: string;
  blockId: string;
};
type SectionMeta = Omit<BlockMeta, 'blockId'>;
type TileMeta = BlockMeta & { tileId: string };

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

export type AddTileToBlock = {
  type: 'ADD_TILE_TO_BLOCK';
  payload: {
    tile: EinTile;
    meta: BlockMeta;
  };
};

export type UpdateTileInBlock = {
  type: 'UPDATE_TILE_IN_BLOCK';
  payload: {
    tile: EinTile;
    meta: TileMeta;
  };
};

export type ReorderTilesInBlock = {
  type: 'REORDER_TILES_IN_BLOCK';
  payload: {
    tiles: EinTile[];
    meta: BlockMeta;
  };
};

export type DeleteTileFromBlock = {
  type: 'DELETE_TILE_FROM_BLOCK';
  payload: {
    meta: TileMeta;
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
  | AddTileToBlock
  | UpdateTileInBlock
  | ReorderTilesInBlock
  | DeleteTileFromBlock
  | UpdateSectionContent
  | AddContentSection
  | SetEducationInNumbersPageContent
  | UpdateContentSection;
