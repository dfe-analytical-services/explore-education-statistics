import ContentHtml from '@common/components/ContentHtml';
import { EinContentBlock } from '@common/services/types/einBlocks';
import React from 'react';
import TileGroupBlock from './TileGroupBlock';

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
