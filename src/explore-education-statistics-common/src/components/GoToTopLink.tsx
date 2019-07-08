import React, { FunctionComponent } from 'react';
import styles from './GoToTopLink.module.scss';

const GoToTopLink: FunctionComponent = () => {
  return (
    <div className={styles.container}>
      <a href="#" className="govuk-link govuk-link--no-visited-state">
        Go to top
      </a>
    </div>
  );
};

export default GoToTopLink;
