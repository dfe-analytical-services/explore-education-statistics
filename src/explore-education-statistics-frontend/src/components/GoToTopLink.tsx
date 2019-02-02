import React, { FunctionComponent } from 'react';

const GoToTopLink: FunctionComponent = () => {
  return (
    <div>
      <a
        href="#application"
        className="govuk-link govuk-link--no-visited-state"
      >
        Go to Top
      </a>
    </div>
  );
};

export default GoToTopLink;
