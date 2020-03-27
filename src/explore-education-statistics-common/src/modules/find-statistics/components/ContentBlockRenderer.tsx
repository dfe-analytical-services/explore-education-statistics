import { ContentBlock } from '@common/services/types/blocks';
import React from 'react';
import ReactMarkdown from 'react-markdown';

interface Props {
  block: ContentBlock;
}

const ContentBlockRenderer = ({ block }: Props) => {
  switch (block.type) {
    case 'MarkDownBlock':
      return <ReactMarkdown className="govuk-body" source={block.body} />;
    case 'HtmlBlock':
      return (
        <div
          // eslint-disable-next-line react/no-danger
          dangerouslySetInnerHTML={{ __html: block.body }}
        />
      );
    default:
      return null;
  }
};

export default ContentBlockRenderer;
