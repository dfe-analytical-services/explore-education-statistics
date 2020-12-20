import useGetChartFile from '@admin/hooks/useGetChartFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import { Block } from '@common/services/types/blocks';
import React from 'react';

interface Props {
  block: Block;
  releaseId: string;
}

const ReleaseBlock = ({ block, releaseId }: Props) => {
  const getChartFile = useGetChartFile(releaseId);

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

  return <ContentBlockRenderer key={block.id} block={block} />;
};

export default ReleaseBlock;
