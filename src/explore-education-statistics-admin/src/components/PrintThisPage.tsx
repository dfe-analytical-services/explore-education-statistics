import React from 'react';
import Button from '@common/components/Button';
import classNames from 'classnames';
import styles from './PrintThisPage.module.scss';

const PrintThisPage = () => {
  return (
    <div
      className={classNames(
        'govuk-!-margin-top-6',
        'dfe-print-hidden',
        styles.mobileHidden,
      )}
    >
      <Button variant="secondary" onClick={() => window.print()}>
        Print this page
      </Button>
    </div>
  );
};

export default PrintThisPage;
