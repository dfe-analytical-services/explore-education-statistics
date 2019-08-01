import { Publication, Release } from '@common/services/publicationService';
import React from 'react';
import ContentSubBlockRenderer from './ContentSubBlockRenderer';

export interface ContentBlockProps {
  content: Release['content'][0]['content'];
  id: string;
  publication: Publication;

  onToggle?: (section: { id: string; title: string }) => void;
}

const ContentBlock = ({
  content,
  id,
  publication,
  onToggle,
}: ContentBlockProps) => {
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
            onToggle={onToggle}
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
