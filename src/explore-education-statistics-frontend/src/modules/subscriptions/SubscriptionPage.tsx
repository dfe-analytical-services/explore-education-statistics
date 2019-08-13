import publicationService, {
  Release,
} from '@common/services/publicationService';
import Page from '@frontend/components/Page';
import functionsService from '@frontend/services/functionsService';
import { NextContext } from 'next';
import React, { Component } from 'react';
import SubscriptionForm, {
  SubscriptionFormSubmitHandler,
} from './components/SubscriptionForm';

interface Props {
  slug: string;
  data: Release;
  unsubscribed: string;
  verified: string;
}

interface State {
  subscribed: boolean;
}

class SubscriptionPage extends Component<Props> {
  public state: State = {
    subscribed: false,
  };

  public static async getInitialProps({
    query,
  }: NextContext<{
    slug: string;
    unsubscribed?: string;
    verified?: string;
  }>) {
    const { slug, unsubscribed, verified } = query;

    const request = publicationService.getPublication(slug);

    const data = await request;

    return {
      data,
      slug,
      unsubscribed,
      verified,
    };
  }

  private handleFormSubmit: SubscriptionFormSubmitHandler = async ({
    email,
  }) => {
    if (email !== '') {
      const {
        data: { id, title },
        slug,
      } = this.props;

      await functionsService.subscribeToPublication({
        email,
        id,
        slug,
        title,
      });
      this.setState({
        subscribed: true,
      });
    }
  };

  public render() {
    const { subscribed } = this.state;
    const { data, slug, unsubscribed, verified } = this.props;

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
        breadcrumbs={[
          { name: 'Find statistics and data', link: '/find-statistics' },
          { name: data.title, link: `/find-statistics/${slug}` },
        ]}
      >
        {message ? (
          <div className="govuk-panel govuk-panel--confirmation">
            <h2 className="govuk-panel__title">{title}</h2>
            <div className="govuk-panel__body">{message}</div>
          </div>
        ) : (
          <>
            <p>Subscribe to receive updates when:</p>
            <ul className="govuk-list govuk-list--bullet">
              <li>new statistics and data are released</li>
              <li>existing statistics and data are changed or corrected</li>
            </ul>
            <SubscriptionForm onSubmit={this.handleFormSubmit} />
          </>
        )}
      </Page>
    );
  }
}

export default SubscriptionPage;
