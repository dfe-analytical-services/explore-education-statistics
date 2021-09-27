import glossaryService from '@admin/services/glossaryService';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import useMethodologyImageAttributeTransformer from '@common/modules/methodology/hooks/useMethodologyImageAttributeTransformer';
import { ContentBlock } from '@common/services/types/blocks';
import React from 'react';

interface Props {
  block: ContentBlock;
  methodologyId: string;
}

const MethodologyBlock = ({ block, methodologyId }: Props) => {
  const transformImageAttributes = useMethodologyImageAttributeTransformer({
    methodologyId,
  });

  return (
    <ContentBlockRenderer
      key={block.id}
      block={block}
      transformImageAttributes={transformImageAttributes}
      getGlossaryEntry={glossaryService.getEntry}
    />
  );
};

export default MethodologyBlock;
