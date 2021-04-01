import ContentSectionIndex from '@common/components/ContentSectionIndex';
import { useMobileMedia } from '@common/hooks/useMedia';
import useMounted from '@common/hooks/useMounted';
import MethodologySectionBlocks from '@frontend/modules/methodologies/components/MethodologySectionBlocks';
import { ContentBlock } from '@common/services/types/blocks';
import React, { useRef } from 'react';

interface MethodologySectionProps {
  content: ContentBlock[];
  open: boolean;
  id: string;
  methodologyId: string;
}

const MethodologyContentSection = ({
  content,
  open,
  id,
  methodologyId,
}: MethodologySectionProps) => {
  const contentRef = useRef<HTMLDivElement>(null);

  const { isMedia: isMobileMedia } = useMobileMedia();

  const { isMounted } = useMounted();

  if (!isMounted) {
    return (
      <MethodologySectionBlocks
        blocks={content}
        methodologyId={methodologyId}
      />
    );
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
        <MethodologySectionBlocks
          blocks={content}
          methodologyId={methodologyId}
        />
      </div>
    </div>
  );
};

export default MethodologyContentSection;
