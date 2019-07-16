import React from 'react';
import classNames from 'classnames';
import {
  AnalyticProps,
  logEvent,
} from '@frontend/services/googleAnalyticsService';

const PrintThisPage = ({ analytics, ...props }: AnalyticProps) => {
  const openPrint = () => {
    if (analytics) {
      logEvent(analytics.category, analytics.action, window.location.pathname);
    }
    window.print();
  };

  return (
    <div className={classNames('govuk-!-margin-top-6', 'dfe-print-hidden')}>
      <a {...props} href="#" onClick={() => openPrint()}>
        Print this page
      </a>
    </div>
  );
};

export default PrintThisPage;
