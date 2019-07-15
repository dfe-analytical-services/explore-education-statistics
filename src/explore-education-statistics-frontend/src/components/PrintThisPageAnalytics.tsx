import React from 'react';
import classNames from 'classnames';
import { logEvent } from '@frontend/services/googleAnalyticsService';

const PrintThisPage = () => {
  return (
    <div className={classNames('govuk-!-margin-top-6', 'dfe-print-hidden')}>
      <a
        href="#"
        onClick={() => {
          window.print();
          logEvent(
            'Page print',
            'Print this page link selected',
            window.location.pathname,
          );
        }}
      >
        Print this page
      </a>
    </div>
  );
};

export default PrintThisPage;
