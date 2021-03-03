import Button from '@common/components/Button';
import React, { MouseEventHandler } from 'react';
import classNames from 'classnames';
import styles from './PrintThisPage.module.scss';

interface Props {
  className?: string;
  onClick?: MouseEventHandler<HTMLButtonElement>;
}

const PrintThisPage = ({ className, onClick }: Props) => {
  return (
    <div
      className={classNames(
        className,
        'dfe-print-hidden',
        styles.printContainer,
        styles.mobileHidden,
      )}
    >
      <Button
        variant="secondary"
        onClick={event => {
          onClick?.(event);
          window.print();
        }}
      >
        Print this page
      </Button>
    </div>
  );
};

export default PrintThisPage;
