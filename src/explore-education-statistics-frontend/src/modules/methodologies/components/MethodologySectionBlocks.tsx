import glossaryService from '@frontend/services/glossaryService';
import InsetText from '@common/components/InsetText';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import { ContentBlock } from '@common/services/types/blocks';
import React from 'react';
import useMethodologyImageAttributeTransformer from '@common/modules/methodology/hooks/useMethodologyImageAttributeTransformer';

interface Props {
  blocks: ContentBlock[];
  methodologyId: string;
}

const MethodologySectionBlocks = ({ blocks, methodologyId }: Props) => {
  const transformImageAttributes = useMethodologyImageAttributeTransformer({
    methodologyId,
    rootUrl: process.env.CONTENT_API_BASE_URL.replace('/api', ''),
  });
  return blocks.length > 0 ? (
    <>
      {blocks.map(block => (
        <ContentBlockRenderer
          key={block.id}
          block={block}
          transformImageAttributes={transformImageAttributes}
          getGlossaryEntry={glossaryService.getEntry}
        />
      ))}
    </>
  ) : (
    <InsetText>There is no content for this section.</InsetText>
  );
};

export default MethodologySectionBlocks;
