import classNames from 'classnames';
import React from 'react';

interface Props {
  className?: string;
}

export default function GoToTopLink({ className }: Props) {
  return (
    <div className={classNames('govuk-!-display-none-print', className)}>
      <a
        href="#main-content"
        className="govuk-link govuk-link--no-visited-state"
      >
        Go to top
      </a>
    </div>
  );
}
