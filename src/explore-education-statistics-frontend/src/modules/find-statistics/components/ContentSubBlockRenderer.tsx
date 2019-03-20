import React from 'react';
import ReactMarkdown from 'react-markdown';
import { ContentBlock } from 'src/services/publicationService';
import { DataBlock } from './DataBlock';

interface Props {
  block: ContentBlock;
}

const ContentSubBlockRenderer = ({ block }: Props) => {
  switch (block.type) {
    case 'MarkDownBlock':
      return <ReactMarkdown className="govuk-body" source={block.body} />;
    case 'InsetTextBlock':
      return (
        <div className="govuk-inset-text">
          {block.heading && (
            <h3 className="govuk-heading-s">{block.heading}</h3>
          )}

          <ReactMarkdown className="govuk-body" source={block.body} />
        </div>
      );
    case 'DataBlock':
      return (
        <div className="dfe-content-overflow">
          <DataBlock {...block} />
        </div>
      );
    default:
      return null;
  }
};

export default ContentSubBlockRenderer;
