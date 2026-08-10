import ContentHtml from '@common/components/ContentHtml';
import TileGroupBlock from '@common/modules/education-in-numbers/components/TileGroupBlock';
import { EinContentBlock } from '@common/services/types/einBlocks';
import React from 'react';

interface Props {
  block: EinContentBlock;
  publicAppUrl: string;
}

const EinContentBlockRenderer = ({ block, publicAppUrl }: Props) => {
  const { type } = block;

  switch (type) {
    case 'HtmlBlock':
      return <ContentHtml html={block.body} />;
    case 'TileGroupBlock':
      return <TileGroupBlock block={block} publicAppUrl={publicAppUrl} />;
    default:
      return null;
  }
};

export default EinContentBlockRenderer;
