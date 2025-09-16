import ContentHtml from '@common/components/ContentHtml';
import TileGroupBlock from '@common/modules/education-in-numbers/TileGroupBlock';
import { EinContentBlock } from '@common/services/types/einBlocks';
import React from 'react';

interface Props {
  block: EinContentBlock;
}

const EinContentBlockRenderer = ({ block }: Props) => {
  const { type } = block;

  switch (type) {
    case 'HtmlBlock':
      return <ContentHtml html={block.body} />;
    case 'TileGroupBlock':
      return <TileGroupBlock block={block} />;
    default:
      return null;
  }
};

export default EinContentBlockRenderer;
