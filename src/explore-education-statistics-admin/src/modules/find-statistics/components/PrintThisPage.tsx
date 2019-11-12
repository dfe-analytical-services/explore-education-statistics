import React, { AnchorHTMLAttributes } from 'react';
import classNames from 'classnames';
import styles from './PrintThisPage.module.scss';

export type PrintThisPageProps = {
  analytics?: unknown;
} & AnchorHTMLAttributes<HTMLAnchorElement>;

const PrintThisPage = ({ ...props }) => {
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
