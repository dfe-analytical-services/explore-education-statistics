import Button from '@common/components/Button';
import React from 'react';
import classNames from 'classnames';
import {
  AnalyticProps,
  logEvent,
} from '@frontend/services/googleAnalyticsService';
import styles from './PrintThisPage.module.scss';

interface Props {
  className?: string;
}

const PrintThisPage = ({ analytics, className }: AnalyticProps & Props) => {
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
        onClick={() => {
          if (analytics) {
            logEvent(
              analytics.category,
              analytics.action,
              window.location.pathname,
            );
          }

          window.print();
        }}
      >
        Print this page
      </Button>
    </div>
  );
};

export default PrintThisPage;
