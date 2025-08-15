import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import { ContentBlock } from '@common/services/types/blocks';
import React from 'react';

interface Props {
  block: ContentBlock;
}

const EducationInNumbersBlock = ({ block }: Props) => {
  return <ContentBlockRenderer key={block.id} block={block} />;
};

export default EducationInNumbersBlock;
