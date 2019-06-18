import { Release } from '@common/services/publicationService';
import React from 'react';
import ContentSubBlockRenderer from './ContentSubBlockRenderer';

interface Props {
  content: Release['content'][0]['content'];
  id: string;
  refreshCallback?: (callback: () => void) => void;
}

const ContentBlock = ({ content, id, refreshCallback }: Props) => {
  return content.length > 0 ? (
    <>
      {content.map((block, index) => {
        const key = `${index}-${block.heading}-${block.type}`;
        return (
          <ContentSubBlockRenderer
            id={id}
            block={block}
            key={key}
            refreshCallback={refreshCallback}
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
