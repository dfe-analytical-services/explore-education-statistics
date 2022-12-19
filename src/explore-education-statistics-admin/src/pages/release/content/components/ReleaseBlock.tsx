import glossaryService from '@admin/services/glossaryService';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import EmbedBlock from '@common/modules/find-statistics/components/EmbedBlock';
import { Block } from '@common/services/types/blocks';
import React from 'react';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import Gate from '@common/components/Gate';

interface Props {
  block: Block;
  releaseId: string;
  visible?: boolean;
}

const ReleaseBlock = ({ block, releaseId, visible }: Props) => {
  const getChartFile = useGetChartFile(releaseId);

  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseId,
  });

  if (block.type === 'EmbedBlockLink') {
    return (
      <Gate condition={!!visible} key={block.id}>
        <EmbedBlock url={block.url} title={block.title} />
      </Gate>
    );
  }

  if (block.type === 'DataBlock') {
    return (
      <Gate condition={!!visible}>
        <DataBlockTabs
          key={block.id}
          dataBlock={block}
          releaseId={releaseId}
          getInfographic={getChartFile}
        />
      </Gate>
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
