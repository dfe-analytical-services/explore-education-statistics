import ContentHtml from '@common/components/ContentHtml';
import TileGroupBlock, {
  TileGroupBlockProps,
} from '@common/modules/education-in-numbers/components/TileGroupBlock';
import { EinContentBlock } from '@common/services/types/einBlocks';
import React from 'react';

interface Props {
  block: EinContentBlock;
  renderLink: TileGroupBlockProps['renderLink'];
}

const EinContentBlockRenderer = ({ block, renderLink }: Props) => {
  const { type } = block;

  switch (type) {
    case 'HtmlBlock':
      return <ContentHtml html={block.body} />;
    case 'TileGroupBlock':
      return <TileGroupBlock block={block} renderLink={renderLink} />;
    default:
      return null;
  }
};

export default EinContentBlockRenderer;
