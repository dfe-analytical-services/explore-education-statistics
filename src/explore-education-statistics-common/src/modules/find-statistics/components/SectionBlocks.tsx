import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs, {
  DataBlockTabsProps,
} from '@common/modules/find-statistics/components/DataBlockTabs';
import { Block } from '@common/services/types/blocks';
import { OmitStrict } from '@common/types';
import React from 'react';

export interface SectionBlocksProps
  extends OmitStrict<DataBlockTabsProps, 'id' | 'dataBlock'> {
  content: Block[];
}

const SectionBlocks = ({ content, ...props }: SectionBlocksProps) => {
  return content.length > 0 ? (
    <>
      {content.map(block => {
        if (block.type === 'DataBlock') {
          return <DataBlockTabs {...props} key={block.id} dataBlock={block} />;
        }

        return <ContentBlockRenderer key={block.id} block={block} />;
      })}
    </>
  ) : (
    <div className="govuk-inset-text">
      There is no content for this section.
    </div>
  );
};

export default SectionBlocks;
