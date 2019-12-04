import React from 'react';
import classNames from 'classnames';
import {
  AnalyticProps,
  logEvent,
} from '@frontend/services/googleAnalyticsService';
import styles from './PrintThisPage.module.scss';

const PrintThisPage = ({ analytics, ...props }: AnalyticProps) => {
  const openPrint = () => {
    if (analytics) {
      logEvent(analytics.category, analytics.action, window.location.pathname);
    }

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const hiddenTabs: any = [];
    const tabs = document.getElementsByClassName('govuk-tabs__panel');
    for (let i = 0; i < tabs.length; i += 1) {
      if (tabs[i].classList.contains('govuk-visually-hidden'))
        hiddenTabs.push(tabs[i]);
      tabs[i].classList.remove('govuk-visually-hidden');
    }

    setTimeout(() => window.print(), 1000);

    setTimeout(function hideTabs() {
      for (let i = 0; i < hiddenTabs.length; i += 1) {
        hiddenTabs[i].classList.add('govuk-visually-hidden');
      }
    }, 1000);
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
