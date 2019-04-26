import { Release } from '@common/services/publicationService';
import React from 'react';
import ContentSubBlockRenderer from './ContentSubBlockRenderer';

interface Props {
  content: Release['content'][0]['content'];
}

const ContentBlock = ({ content }: Props) => {
  return content.length > 0 ? (
    <>
      {content.map((block, index) => {
        const key = `${index}-${block.heading}-${block.type}`;
        return <ContentSubBlockRenderer block={block} key={key} />;
      })}
    </>
  ) : (
    <div className="govuk-inset-text">
      There is no content for this section.
    </div>
  );
};

export default ContentBlock;
