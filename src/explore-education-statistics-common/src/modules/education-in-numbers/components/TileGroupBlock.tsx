import FreeTextStatTile from '@common/modules/education-in-numbers/components/FreeTextStatTile';
import TileWrapper from '@common/modules/education-in-numbers/components/TileWrapper';
import { EinTileGroupBlock } from '@common/services/types/einBlocks';
import React from 'react';
import ApiQueryStatTile, {
  ApiQueryStatTileProps,
} from '@common/modules/education-in-numbers/components/ApiQueryStatTile';

export interface TileGroupBlockProps {
  block: EinTileGroupBlock;
  renderLink: ApiQueryStatTileProps['renderLink'];
}

const TileGroupBlock = ({ block, renderLink }: TileGroupBlockProps) => {
  const { title, tiles } = block;

  return (
    <div className="govuk-!-margin-bottom-3">
      {title && (
        <h3 className="govuk-!-margin-bottom-4 govuk-!-margin-top-8">
          {title}
        </h3>
      )}
      <TileWrapper>
        {tiles.map(tile => {
          switch (tile.type) {
            case 'FreeTextStatTile':
              return <FreeTextStatTile key={tile.id} tile={tile} />;
            case 'ApiQueryStatTile':
              return (
                <ApiQueryStatTile
                  key={tile.id}
                  tile={tile}
                  renderLink={renderLink}
                />
              );
            default:
              return null;
          }
        })}
      </TileWrapper>
    </div>
  );
};

export default TileGroupBlock;
