import SectionBreak from '@common/components/SectionBreak';
import { useMobileMedia } from '@common/hooks/useMedia';
import { PageSectionId } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React, { ReactNode } from 'react';
import styles from './DataSetFilePageSection.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  heading: string;
  id: PageSectionId;
  testId?: string;
}

export default function DataSetFilePageSection({
  children,
  className,
  heading,
  id,
  testId,
}: Props) {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <>
      <section
        className={className}
        id={id}
        data-page-section=""
        data-testid={testId}
      >
        <h2>{heading}</h2>

        <div className={styles.content}>
          {children}

          {isMobileMedia && (
            <p className="govuk-!-margin-top-8">
              <a className="govuk-link--no-visited-state" href="#top">
                Back to top
              </a>
            </p>
          )}
        </div>
      </section>

      <SectionBreak size="xl" />
    </>
  );
}
