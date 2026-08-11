import FreeTextStatTile from '@common/modules/education-in-numbers/components/FreeTextStatTile';
import TileWrapper from '@common/modules/education-in-numbers/components/TileWrapper';
import { EinTileGroupBlock } from '@common/services/types/einBlocks';
import React from 'react';
import ApiQueryStatTile from '@common/modules/education-in-numbers/components/ApiQueryTextStatTile';

export interface TileGroupBlockProps {
  block: EinTileGroupBlock;
  publicAppUrl: string;
}

const TileGroupBlock = ({ block, publicAppUrl }: TileGroupBlockProps) => {
  const { title, tiles } = block;

  return (
    <div className="govuk-!-margin-bottom-3">
      {title && (
        <h3 className="govuk-!-margin-bottom-4 govuk-!-margin-top-8">
          {title}
        </h3>
      )}
      {tiles.map(tile => {
        switch (tile.type) {
          case 'FreeTextStatTile':
            return (
              <TileWrapper key={tile.id}>
                <FreeTextStatTile tile={tile} />
              </TileWrapper>
            );
          case 'ApiQueryStatTile':
            return (
              <TileWrapper key={tile.id}>
                <ApiQueryStatTile tile={tile} publicAppUrl={publicAppUrl} />
              </TileWrapper>
            );
          default:
            return null;
        }
      })}
    </div>
  );
};

export default TileGroupBlock;
