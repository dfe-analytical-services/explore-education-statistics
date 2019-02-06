import React, { FunctionComponent } from 'react';

const GoToTopLink: FunctionComponent = () => {
  return (
    <div>
      <a
        href="#application"
        className="govuk-link govuk-link--no-visited-state"
      >
        Go to top
      </a>
    </div>
  );
};

export default GoToTopLink;
