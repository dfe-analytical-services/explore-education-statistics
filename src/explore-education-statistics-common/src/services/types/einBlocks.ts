import { BaseBlock, HtmlBlock } from '@common/services/types/blocks';
import { ContentSection } from '@common/services/publicationService';

// Generic EiN block types
export type EinContentBlock = EinHtmlBlock | EinTileGroupBlock;
export type EinBlockType = EinContentBlock['type'];

// WARN: Even though they share the same type, data is returned from two different db tables:
// - HtmlBlock come from the table ContentBlocks (used for releases/methodologies)
// - EinHtmlBlock comes from EinContentBlocks (used for EiN exclusively)
// We create EinHtmlBlock here to represent that on the frontend.
export type EinHtmlBlock = HtmlBlock;

export type EinFreeTextStatTile = {
  id: string;
  type: 'FreeTextStatTile';
  order: number;
  title: string;
  statistic: string;
  trend: string;
  linkUrl?: string;
  linkText?: string;
};

export interface EinTileGroupBlock extends BaseBlock {
  type: 'TileGroupBlock';
  tiles: EinFreeTextStatTile[];
  title?: string;
}

// ContentSection is shared with release/methodology pages too
export type EinContentSection = ContentSection<EinContentBlock>;
