import GoToTopLink from '@common/components/GoToTopLink';
import SectionBreak from '@common/components/SectionBreak';
import generateIdFromHeading from '@common/components/util/generateIdFromHeading';
import { useMobileMedia } from '@common/hooks/useMedia';
import React, { ReactNode } from 'react';

interface Props {
  caption?: string;
  children: ReactNode;
  className?: string;
  heading: string;
  id: string;
  includeBackToTopLink?: boolean;
  includeSectionBreak?: boolean;
  testId?: string;
}

export default function ReleasePageContentSection({
  caption,
  children,
  className,
  heading,
  id,
  includeSectionBreak = true,
  includeBackToTopLink = true,
  testId,
}: Props) {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <>
      <section
        className={className}
        id={id}
        data-page-section
        data-testid={testId}
      >
        <h2
          id={generateIdFromHeading(heading)}
          className={caption ? 'govuk-!-margin-bottom-4' : undefined}
        >
          {heading}
        </h2>

        {caption && <p>{caption}</p>}

        <div>
          {children}

          {isMobileMedia && includeBackToTopLink && (
            <GoToTopLink className="govuk-!-margin-top-8" />
          )}
        </div>
      </section>

      {includeSectionBreak && <SectionBreak size="xl" />}
    </>
  );
}
