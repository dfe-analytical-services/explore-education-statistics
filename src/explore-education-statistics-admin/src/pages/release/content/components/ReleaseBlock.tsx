import glossaryService from '@admin/services/glossaryService';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import { Block } from '@common/services/types/blocks';
import React from 'react';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';

interface Props {
  block: Block;
  releaseId: string;
}

const ReleaseBlock = ({ block, releaseId }: Props) => {
  const getChartFile = useGetChartFile(releaseId);

  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseId,
  });

  if (block.type === 'DataBlock') {
    return (
      <DataBlockTabs
        key={block.id}
        dataBlock={block}
        releaseId={releaseId}
        getInfographic={getChartFile}
      />
    );
  }

  return (
    <ContentBlockRenderer
      key={block.id}
      block={block}
      transformImageAttributes={transformImageAttributes}
      getGlossaryEntry={glossaryService.getEntry}
    />
  );
};

export default ReleaseBlock;
