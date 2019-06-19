import { Publication, Release } from '@common/services/publicationService';
import React from 'react';
import ContentSubBlockRenderer from './ContentSubBlockRenderer';

interface Props {
  content: Release['content'][0]['content'];
  id: string;
  publication: Publication;
}

const ContentBlock = ({ content, id, publication }: Props) => {
  return content.length > 0 ? (
    <>
      {content.map((block, index) => {
        const key = `${index}-${block.heading}-${block.type}`;
        return (
          <ContentSubBlockRenderer
            id={id}
            block={block}
            key={key}
            publication={publication}
          />
        );
      })}
    </>
  ) : (
    <div className="govuk-inset-text">
      There is no content for this section.
    </div>
  );
};

export default ContentBlock;
