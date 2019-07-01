import publicationService, {
  Release,
} from '@common/services/publicationService';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
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

    if (unsubscribed) {
      message = 'You have successfully unsubscribed from these updates.';
    } else if (verified) {
      message = 'You have successfully subscribed to these updates.';
    } else if (subscribed) {
      message = 'Thank you. Check your email to confirm your subscription.';
    }

    return (
      <Page
        breadcrumbs={[
          { name: 'Find statistics and data', link: '/statistics' },
          { name: data.title, link: `/statistics/${slug}` },
          { name: 'Notify me' },
        ]}
        pageMeta={{ title: data.title, description: 'Subscribe' }}
      >
        <PageTitle title={`${data.title}`} caption="Notify me" />

        <div className="govuk-warning-text">
          <span className="govuk-warning-text__icon" aria-hidden="true">
            !
          </span>
          <strong className="govuk-warning-text__text">
            <span className="govuk-warning-text__assistive">Warning</span>
            This feature is not currently available for use.
          </strong>
        </div>

        <p>Subscribe to receive updates when:</p>
        <ul className="govuk-list govuk-list--bullet">
          <li>new statistics and data are released</li>
          <li>existing statistics and data are changed or corrected</li>
        </ul>

        {message ? (
          <p>{message}</p>
        ) : (
          <SubscriptionForm onSubmit={this.handleFormSubmit} />
        )}
      </Page>
    );
  }
}

export default SubscriptionPage;
