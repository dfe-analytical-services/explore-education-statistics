import GoToTopLink from '@common/components/GoToTopLink';
import SectionBreak from '@common/components/SectionBreak';
import generateIdFromHeading from '@common/components/util/generateIdFromHeading';
import { useMobileMedia } from '@common/hooks/useMedia';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  className?: string;
  heading: string;
  id: string;
  includeSectionBreak?: boolean;
  testId?: string;
}

export default function ReleasePageContentSection({
  children,
  className,
  heading,
  id,
  includeSectionBreak = true,
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
        <h2 id={generateIdFromHeading(heading)}>{heading}</h2>

        <div>
          {children}

          {isMobileMedia && <GoToTopLink className="govuk-!-margin-top-8" />}
        </div>
      </section>

      {includeSectionBreak && <SectionBreak size="xl" />}
    </>
  );
}
