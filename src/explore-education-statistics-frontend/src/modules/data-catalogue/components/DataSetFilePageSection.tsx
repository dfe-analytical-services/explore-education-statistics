import { useMobileMedia } from '@common/hooks/useMedia';
import { PageSection } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  heading: string;
  id: PageSection;
}
export default function DataSetFilePageSection({
  children,
  heading,
  id,
}: Props) {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div
      className="dfe-border-bottom govuk-!-margin-bottom-6"
      id={id}
      data-scroll
    >
      <h2>{heading}</h2>
      {children}

      {isMobileMedia && (
        <p>
          <a className="govuk-link--no-visited-state" href="#main-content">
            Back to top
          </a>
        </p>
      )}
    </div>
  );
}
