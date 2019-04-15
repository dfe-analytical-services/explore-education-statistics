import publicationService, {
  Release,
} from '@common/services/publicationService';
import { NextContext } from 'next';
import React, { Component } from 'react';
import Page from 'src/components/Page';
import PageTitle from 'src/components/PageTitle';
import functionsService from 'src/services/functionsService';
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
  id: string;
  slug: string;
  title: string;
  subscribed: boolean;
}

class SubscriptionPage extends Component<Props> {
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

  public state: State = {
    id: this.props.data.id,
    slug: this.props.slug,
    subscribed: false,
    title: this.props.data.title,
  };

  private handleFormSubmit: SubscriptionFormSubmitHandler = async ({
    email,
  }) => {
    if (email !== '') {
      const slug = this.state.slug;
      const title = this.state.title;
      const id = this.state.id;

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
    const { data, slug, unsubscribed, verified } = this.props;
    let message;

    if (unsubscribed) {
      message = 'You have successfully unsubscribed from this publication.';
    } else if (verified) {
      message = 'You have successfully subscribed to this publication.';
    } else if (this.state.subscribed) {
      message =
        'Thank you. Please check your email to verify the subscription.';
    }

    return (
      <Page
        breadcrumbs={[
          { name: 'Find statistics and data', link: '/statistics' },
          { name: data.title, link: `/statistics/${slug}` },
          { name: 'subscribe' },
        ]}
      >
        <PageTitle title={`${data.title}`} caption={'Subscription'} />

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
