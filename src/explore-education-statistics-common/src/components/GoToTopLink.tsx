import React, { FunctionComponent } from 'react';

const GoToTopLink: FunctionComponent = () => {
  return (
    <div className="dfe-print-hidden">
      <a
        href="#main-content"
        className="govuk-link govuk-link--no-visited-state"
      >
        Go to top
      </a>
    </div>
  );
};

export default GoToTopLink;
