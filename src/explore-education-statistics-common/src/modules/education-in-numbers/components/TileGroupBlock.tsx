import FreeTextStatTile from '@common/modules/education-in-numbers/components/FreeTextStatTile';
import FreeTextStatTileWrapper from '@common/modules/education-in-numbers/components/FreeTextStatTileWrapper';
import { EinTileGroupBlock } from '@common/services/types/einBlocks';
import React from 'react';

export interface TileGroupBlockProps {
  block: EinTileGroupBlock;
}

const TileGroupBlock = ({ block }: TileGroupBlockProps) => {
  const { title, tiles } = block;

  return (
    <div className="govuk-!-margin-bottom-3">
      {title && (
        <h3 className="govuk-!-margin-bottom-4 govuk-!-margin-top-8">
          {title}
        </h3>
      )}
      <FreeTextStatTileWrapper>
        {tiles.map(tile => (
          <FreeTextStatTile key={tile.id} tile={tile} />
        ))}
      </FreeTextStatTileWrapper>
    </div>
  );
};

export default TileGroupBlock;
