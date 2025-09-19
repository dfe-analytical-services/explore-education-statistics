import { EinTileGroupBlock } from '@common/services/types/einBlocks';
import testTile from './testTile';

const testBlock: EinTileGroupBlock = {
  type: 'TileGroupBlock',
  id: 'tile-group-block-1',
  order: 0,
  tiles: [testTile],
  title: 'Test Tile Group Block Title',
};

export default testBlock;
