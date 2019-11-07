import React from 'react';
import classNames from 'classnames';
import styles from './PrintThisPage.module.scss';

const PrintThisPage = ({...props}) => {
  const openPrint = () => {
    window.print();
  };

  return (
    <div
      className={classNames(
        'govuk-!-margin-top-6',
        'dfe-print-hidden',
        styles.mobileHidden,
      )}
    >
      <a
        className="govuk-button govuk-button--secondary"
        {...props}
        href="#"
        onClick={() => openPrint()}
      >
        Print this page
      </a>
    </div>
  );
};

export default PrintThisPage;
