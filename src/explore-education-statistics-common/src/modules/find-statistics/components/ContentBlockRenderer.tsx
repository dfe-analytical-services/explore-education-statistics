import { ContentBlock } from '@common/services/types/blocks';
import React from 'react';
import ReactMarkdown from 'react-markdown';
import SanitizeHtml from '@common/components/SanitizeHtml';

interface Props {
  block: ContentBlock;
}

const ContentBlockRenderer = ({ block }: Props) => {
  switch (block.type) {
    case 'MarkDownBlock':
      return <ReactMarkdown className="govuk-body" source={block.body} />;
    case 'HtmlBlock':
      return <SanitizeHtml dirtyHtml={block.body} />;
    default:
      return null;
  }
};

export default ContentBlockRenderer;
