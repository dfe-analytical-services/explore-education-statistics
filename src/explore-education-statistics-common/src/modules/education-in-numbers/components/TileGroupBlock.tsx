import FreeTextStatTile from '@common/modules/education-in-numbers/components/FreeTextStatTile';
import { EinTileGroupBlock } from '@common/services/types/einBlocks';
import React from 'react';

export interface TileGroupBlockProps {
  block: EinTileGroupBlock;
}

const TileGroupBlock = ({ block }: TileGroupBlockProps) => {
  const { title, tiles } = block;

  return (
    <div>
      {title && <h2>{title}</h2>}
      {tiles?.map(tile => <FreeTextStatTile key={tile.id} tile={tile} />)}
    </div>
  );
};

export default TileGroupBlock;
