import React from 'react';
import classNames from 'classnames';
import Link from '@frontend/components/Link';
import styles from '@frontend/modules/subscriptions/SubscriptionPage.module.scss';

interface SubscriptionStatusMessageProps {
  title: string;
  message: string;
  slug?: string;
}

const SubscriptionStatusMessage = ({
  title,
  message,
  slug,
}: SubscriptionStatusMessageProps) => {
  return (
    <div
      className={classNames(
        'govuk-panel',
        'govuk-panel--confirmation',
        styles.panelContainer,
      )}
    >
      <h1 className="govuk-panel__title">{title}</h1>
      <div className="govuk-panel__body">{message}</div>
      {slug && <Link to={`/find-statistics/${slug}`}>View {title}</Link>}
    </div>
  );
};

export default SubscriptionStatusMessage;
