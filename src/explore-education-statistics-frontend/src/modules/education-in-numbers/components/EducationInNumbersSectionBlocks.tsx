import InsetText from '@common/components/InsetText';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import { ContentBlock } from '@common/services/types/blocks';
import React from 'react';

interface Props {
  blocks: ContentBlock[];
}

const EducationInNumbersSectionBlocks = ({ blocks }: Props) => {
  return blocks.length > 0 ? (
    <>
      {blocks.map(block => (
        <ContentBlockRenderer key={block.id} block={block} />
      ))}
    </>
  ) : (
    <InsetText>There is no content for this section.</InsetText>
  );
};

export default EducationInNumbersSectionBlocks;
