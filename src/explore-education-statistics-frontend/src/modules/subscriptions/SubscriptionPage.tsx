import publicationService, {
  PublicationTitle,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import functionsService from '@frontend/services/functionsService';
import classNames from 'classnames';
import { NextPage } from 'next';
import React, { useState } from 'react';
import SubscriptionForm, {
  SubscriptionFormSubmitHandler,
} from './components/SubscriptionForm';
import styles from './SubscriptionPage.module.scss';

interface Props {
  slug: string;
  data: PublicationTitle;
  unsubscribed?: string;
  verified?: string;
}

const SubscriptionPage: NextPage<Props> = ({
  data,
  slug,
  unsubscribed = '',
  verified = '',
}) => {
  const [subscribed, setSubscribed] = useState(false);

  const handleFormSubmit: SubscriptionFormSubmitHandler = async ({ email }) => {
    if (email !== '') {
      const { id, title } = data;

      await functionsService.subscribeToPublication({
        email,
        id,
        slug,
        title,
      });

      setSubscribed(true);
    }
  };

  let message;
  let title;

  if (unsubscribed) {
    title = 'Unsubscribed';
    message = 'You have successfully unsubscribed from these updates.';
  } else if (verified) {
    title = 'Subscription verified';
    message = 'You have successfully subscribed to these updates.';
  } else if (subscribed) {
    title = 'Subscribed';
    message = 'Thank you. Check your email to confirm your subscription.';
  }

  return (
    <Page
      title={data.title}
      caption="Notify me"
      breadcrumbLabel="Notify me"
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        { name: data.title, link: `/find-statistics/${slug}` },
      ]}
    >
      {message ? (
        <div
          className={classNames(
            'govuk-panel',
            'govuk-panel--confirmation',
            styles.panelContainer,
          )}
        >
          <h1 className="govuk-panel__title">{title}</h1>
          <div className="govuk-panel__body">{message}</div>
          {verified && (
            <Link to={`/find-statistics/${slug}`}>View {data.title}</Link>
          )}
        </div>
      ) : (
        <>
          <p>Subscribe to receive updates when:</p>
          <ul className="govuk-list govuk-list--bullet">
            <li>new statistics and data are released</li>
            <li>existing statistics and data are changed or corrected</li>
          </ul>
          <SubscriptionForm onSubmit={handleFormSubmit} />
        </>
      )}
    </Page>
  );
};

SubscriptionPage.getInitialProps = async ({ query }) => {
  const { slug, unsubscribed, verified } = query as Dictionary<string>;

  const request = publicationService.getPublicationTitle(slug as string);

  const data = await request;

  return {
    data,
    slug,
    unsubscribed,
    verified,
  };
};

export default SubscriptionPage;
