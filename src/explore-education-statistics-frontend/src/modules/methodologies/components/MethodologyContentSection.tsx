import ContentSectionIndex from '@common/components/ContentSectionIndex';
import { useMobileMedia } from '@common/hooks/useMedia';
import useMounted from '@common/hooks/useMounted';
import MethodologySectionBlocks from '@frontend/modules/methodologies/components/MethodologySectionBlocks';
import { ContentBlock } from '@common/services/types/blocks';
import React, { useRef, useState } from 'react';

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

  const [hasIndex, setHasIndex] = useState<boolean>(false);

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
      <ContentSectionIndex
        id={id}
        className="govuk-grid-column-one-quarter"
        contentRef={contentRef}
        sticky={open}
        visible={!isMobileMedia}
        onMount={setHasIndex}
      />

      <div
        className={
          hasIndex
            ? 'govuk-grid-column-three-quarters'
            : 'govuk-grid-column-full'
        }
        ref={contentRef}
      >
        <MethodologySectionBlocks
          blocks={content}
          methodologyId={methodologyId}
        />
      </div>
    </div>
  );
};

export default MethodologyContentSection;
