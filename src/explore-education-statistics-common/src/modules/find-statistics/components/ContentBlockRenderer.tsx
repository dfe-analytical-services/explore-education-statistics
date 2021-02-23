import { ContentBlock } from '@common/services/types/blocks';
import React from 'react';
import ReactMarkdown from 'react-markdown';
import SanitizeHtml from '@common/components/SanitizeHtml';

interface Props {
  block: ContentBlock;
}

const ContentBlockRenderer = ({ block }: Props) => {
  const { body = '' } = block;

  switch (block.type) {
    case 'MarkDownBlock':
      return <ReactMarkdown className="dfe-content" source={body} />;
    case 'HtmlBlock':
      return <SanitizeHtml dirtyHtml={body} />;
    default:
      return null;
  }
};

export default ContentBlockRenderer;
