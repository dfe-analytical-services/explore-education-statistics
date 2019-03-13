import React from 'react';
import ReactMarkdown from 'react-markdown';
import { Release } from 'src/services/publicationService';

interface Props {
  block: Release['content'][0]['content'][0];
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
    default:
      return null;
  }
};

export default ContentSubBlockRenderer;
