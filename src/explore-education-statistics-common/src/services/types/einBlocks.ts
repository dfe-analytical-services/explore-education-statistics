import { ContentSection } from '@common/services/publicationService';

export interface EinBaseBlock {
  id: string;
  order: number;
  type: EinBlockType;
}

export type EinContentBlock = EinHtmlBlock | EinTileGroupBlock;
export type EinBlockType = EinContentBlock['type'];

export interface EinHtmlBlock extends EinBaseBlock {
  type: 'HtmlBlock';
  body: string;
}

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

export interface EinTileGroupBlock extends EinBaseBlock {
  type: 'TileGroupBlock';
  tiles: EinFreeTextStatTile[];
  title?: string;
}

// ContentSection is shared with release/methodology pages too
export type EinContentSection = ContentSection<EinContentBlock>;
