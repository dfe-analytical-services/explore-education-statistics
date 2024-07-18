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
  releaseVersionId: string;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
  visible?: boolean;
}

const ReleaseBlock = ({
  block,
  releaseVersionId,
  transformFeaturedTableLinks,
  visible,
}: Props) => {
  const getChartFile = useGetChartFile(releaseVersionId);

  // TODO rename to releaseVersionId
  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseId: releaseVersionId,
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
      // TODO rename to releaseVersionId
      <Gate condition={!!visible}>
        <DataBlockTabs
          key={block.id}
          dataBlock={block}
          releaseId={releaseVersionId}
          getInfographic={getChartFile}
        />
      </Gate>
    );
  }

  return (
    <ContentBlockRenderer
      key={block.id}
      block={block}
      transformFeaturedTableLinks={transformFeaturedTableLinks}
      transformImageAttributes={transformImageAttributes}
      getGlossaryEntry={glossaryService.getEntry}
    />
  );
};

export default ReleaseBlock;
