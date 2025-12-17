import classNames from 'classnames';
import React from 'react';

interface Props {
  className?: string;
}

export default function BackToTopLink({ className }: Props) {
  return (
    <div className={classNames('govuk-!-display-none-print', className)}>
      <a href="#top" className="govuk-link govuk-link--no-visited-state">
        Back to top
      </a>
    </div>
  );
}
