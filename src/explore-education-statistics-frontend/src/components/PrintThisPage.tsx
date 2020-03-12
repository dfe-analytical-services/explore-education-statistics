import React from 'react';
import classNames from 'classnames';
import {
  AnalyticProps,
  logEvent,
} from '@frontend/services/googleAnalyticsService';
import styles from './PrintThisPage.module.scss';

interface Props {
  noMargin?: boolean;
  alignCentre?: boolean;
}

const PrintThisPage = ({
  analytics,
  noMargin,
  alignCentre,
  ...props
}: AnalyticProps & Props) => {
  const openPrint = () => {
    if (analytics) {
      logEvent(analytics.category, analytics.action, window.location.pathname);
    }

    window.print();
  };

  return (
    <div
      className={classNames(
        {
          'govuk-!-margin-top-6': !noMargin,
          [styles.alignCentre]: alignCentre,
        },
        'dfe-print-hidden',
        styles.printContainer,
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
