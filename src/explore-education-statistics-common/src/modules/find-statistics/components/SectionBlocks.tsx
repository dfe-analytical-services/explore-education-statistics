import { Publication } from '@common/services/publicationService';
import { Block } from '@common/services/types/blocks';
import React from 'react';
import BlockRenderer, { SectionToggleHandler } from './BlockRenderer';

export interface BlocksProps {
  content: Block[];
  id: string;
  publication?: Publication;
  onToggle?: SectionToggleHandler;
}

const SectionBlocks = ({ content, id, publication, onToggle }: BlocksProps) => {
  return content?.length > 0 ? (
    <>
      {content.map(block => {
        return (
          <BlockRenderer
            id={id}
            block={block}
            key={block.id}
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

export default SectionBlocks;
