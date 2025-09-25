import InsetText from '@common/components/InsetText';
import EinContentBlockRenderer from '@common/modules/education-in-numbers/components/EinContentBlockRenderer';
import { EinContentBlock } from '@common/services/types/einBlocks';
import React from 'react';

interface Props {
  blocks: EinContentBlock[];
}

const EducationInNumbersSectionBlocks = ({ blocks }: Props) => {
  return blocks.length > 0 ? (
    <>
      {blocks.map(block => (
        <EinContentBlockRenderer key={block.id} block={block} />
      ))}
    </>
  ) : (
    <InsetText>There is no content for this section.</InsetText>
  );
};

export default EducationInNumbersSectionBlocks;
