import { useMobileMedia } from '@common/hooks/useMedia';
import useMounted from '@common/hooks/useMounted';
import SectionBlocks from '@common/modules/find-statistics/components/SectionBlocks';
import { Block } from '@common/services/types/blocks';
import ContentSectionIndex from '@common/components/ContentSectionIndex';
import React, { useRef } from 'react';

interface MethodologySectionProps {
  content: Block[];
  open: boolean;
  id: string;
}

const MethodologyContentSection = ({
  content,
  open,
  id,
}: MethodologySectionProps) => {
  const contentRef = useRef<HTMLDivElement>(null);

  const { isMedia: isMobileMedia } = useMobileMedia();

  const { isMounted } = useMounted();

  if (!isMounted) {
    return <SectionBlocks content={content} />;
  }

  return (
    <div className="govuk-grid-row">
      <div className="govuk-grid-column-one-quarter">
        <ContentSectionIndex
          id={id}
          contentRef={contentRef}
          sticky={open}
          visible={!isMobileMedia}
        />
      </div>

      <div className="govuk-grid-column-three-quarters" ref={contentRef}>
        <SectionBlocks content={content} />
      </div>
    </div>
  );
};

export default MethodologyContentSection;
