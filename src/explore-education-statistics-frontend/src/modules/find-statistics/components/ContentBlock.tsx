import React from 'react';
import { Release } from '../../../services/publicationService';
import ContentSubBlockRenderer from './ContentSubBlockRenderer';

interface Props {
  content: Release['content'][0]['content'];
}

const ContentBlock = ({ content }: Props) => {
  return content.length > 0 ? (
    <>
      {content.map((block, index) => (
        <ContentSubBlockRenderer
          block={block}
          key={`${index}-${block.heading}-${block.type}`}
        />
      ))}
    </>
  ) : (
    <div className="govuk-inset-text">
      There is no content for this section.
    </div>
  );
};

export default ContentBlock;
