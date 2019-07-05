import React, { FunctionComponent } from 'react';
import printStyles from '@frontend/components/PrintCSS.module.scss';

const GoToTopLink: FunctionComponent = () => {
  return (
    <div className={printStyles.hidden}>
      <a href="#" className="govuk-link govuk-link--no-visited-state">
        Go to top
      </a>
    </div>
  );
};

export default GoToTopLink;
