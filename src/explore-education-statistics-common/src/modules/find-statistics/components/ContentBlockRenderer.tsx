import { ContentBlock } from '@common/services/types/blocks';
import React from 'react';
import ReactMarkdown from 'react-markdown';
import ContentHtml from '@common/components/ContentHtml';

interface Props {
  block: ContentBlock;
}

const ContentBlockRenderer = ({ block }: Props) => {
  const { body = '' } = block;

  switch (block.type) {
    case 'MarkDownBlock':
      return <ReactMarkdown className="dfe-content" source={body} />;
    case 'HtmlBlock':
      return <ContentHtml html={body} />;
    default:
      return null;
  }
};

export default ContentBlockRenderer;
