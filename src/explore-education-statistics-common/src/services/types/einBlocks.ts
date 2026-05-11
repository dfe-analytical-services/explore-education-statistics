import { ContentSection } from '@common/services/publicationService';

export interface EinBaseBlock {
  id: string;
  order: number;
  type: EinBlockType;
}

export type EinContentBlock = EinHtmlBlock | EinTileGroupBlock;
export type EinBlockType = EinContentBlock['type'];

export type EinTile = EinFreeTextStatTile | EinApiQueryStatTile;
export type EinTileType = EinTile['type'];

export interface EinHtmlBlock extends EinBaseBlock {
  type: 'HtmlBlock';
  body: string;
}

export interface EinBaseTile {
  id: string;
  title: string;
  order: number;
}

export interface EinFreeTextStatTile extends EinBaseTile {
  type: 'FreeTextStatTile';
  statistic: string;
  trend: string;
  linkUrl?: string;
  linkText?: string;
}

export interface EinApiQueryStatTile extends EinBaseTile {
  type: 'ApiQueryStatTile';
  dataSetId: string;
  version?: string;
  isLatestVersion: boolean;
  query?: string;
  statistic?: string;
  indicatorUnit?: string; // @MarkFix indicator unit type?
  decimalPlaces?: number;
  publicationSlug?: string;
  releaseSlug?: string;
}

export interface EinTileGroupBlock extends EinBaseBlock {
  type: 'TileGroupBlock';
  tiles: EinTile[];
  title?: string;
}

// ContentSection is shared with release/methodology pages too
export type EinContentSection = ContentSection<EinContentBlock>;
