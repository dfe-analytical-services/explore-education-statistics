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
      {tiles?.map(tile => (
        <div key={tile.id}>
          <h3>{tile.title}</h3>
          <p>{tile.trend}</p>
          <p>{tile.statistic}</p>
          <p>{tile.linkText}</p>
          <p>{tile.linkUrl}</p>
        </div>
      ))}
    </div>
  );
};

export default TileGroupBlock;
