import { useMobileMedia } from '@common/hooks/useMedia';
import {
  PageHiddenSectionId,
  PageSectionId,
} from '@frontend/modules/data-catalogue/DataSetFilePage';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  className?: string;
  heading: string;
  id: PageSectionId | PageHiddenSectionId;
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
    <div
      className={classNames(
        'dfe-border-bottom govuk-!-margin-bottom-6',
        className,
      )}
      id={id}
      data-page-section=""
      data-testid={testId}
    >
      <h2>{heading}</h2>
      {children}

      {isMobileMedia && (
        <p className="govuk-!-margin-top-3">
          <a className="govuk-link--no-visited-state" href="#main-content">
            Back to top
          </a>
        </p>
      )}
    </div>
  );
}
