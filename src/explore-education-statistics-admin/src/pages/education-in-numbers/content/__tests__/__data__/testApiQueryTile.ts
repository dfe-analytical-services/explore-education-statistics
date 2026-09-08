import { EinApiQueryStatTile } from '@common/services/types/einBlocks';

const testApiQueryTile: EinApiQueryStatTile = {
  id: 'tile-2',
  type: 'ApiQueryStatTile',
  order: 0,
  title: 'Tile 2 title',
  dataSetId: 'b8e0cbc4-e1f8-4b32-9d0f-8d0c5d3f0a11',
  version: '1.0.1',
  isLatestVersion: true,
  query: '{ "indicators": ["tile-2-indicator"] }',
  statistic: '1000',
  indicatorUnit: '%',
  decimalPlaces: 1,
  publicationSlug: 'tile-2-publication-slug',
  releaseSlug: 'tile-2-release-slug',
  publicationLabel: 'Tile 2 publication',
  releaseLabel: 'Academic year 2023/24',
};

export default testApiQueryTile;
