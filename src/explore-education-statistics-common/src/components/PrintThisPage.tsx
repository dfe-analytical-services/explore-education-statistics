import React from 'react';
import classNames from 'classnames';
import styles from './PrintThisPage.module.scss';

const PrintThisPage = () => {
  return (
    <div className={classNames('govuk-!-margin-top-6', styles.container)}>
      <a href="#" onClick={() => window.print()}>
        Print this page
      </a>
    </div>
  );
};

export default PrintThisPage;
