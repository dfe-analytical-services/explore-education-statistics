import BackToTopLink from '@common/components/BackToTopLink';
import SectionBreak from '@common/components/SectionBreak';
import generateIdFromHeading from '@common/components/util/generateIdFromHeading';
import React, { ReactNode } from 'react';

interface Props {
  caption?: string;
  children: ReactNode;
  className?: string;
  dataScrollId?: string;
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
  dataScrollId,
  heading,
  id,
  includeSectionBreak = true,
  includeBackToTopLink = true,
  testId,
}: Props) {
  return (
    <>
      <section
        className={className}
        id={id}
        data-scroll={dataScrollId}
        data-testid={testId}
      >
        <h2
          id={generateIdFromHeading(heading, 'heading')}
          className={caption ? 'govuk-!-margin-bottom-4' : undefined}
        >
          {heading}
        </h2>

        {caption && <p>{caption}</p>}

        <div>
          {children}

          {includeBackToTopLink && (
            <BackToTopLink className="govuk-!-margin-top-8" />
          )}
        </div>
      </section>

      {includeSectionBreak && <SectionBreak size="xl" />}
    </>
  );
}
